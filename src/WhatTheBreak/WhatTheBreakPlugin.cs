using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using MonoMod.Utils;

using UnityEngine;
using UnityEngine.UI;

namespace WhatTheBreak
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public sealed class WhatTheBreakPlugin : BaseUnityPlugin
    {
        readonly Dictionary<string, ExceptionData> _exceptionMap = new Dictionary<string, ExceptionData>(StringComparer.Ordinal);

        Dictionary<string, List<MethodBase>> _patchMap = null;

        void OnEnable()
        {
            Application.logMessageReceived += OnLogReceived;

            Logger.LogInfo("Loaded " + nameof(WhatTheBreakPlugin));
        }

        sealed class OwnerData
        {
            public readonly StringBuilder Patches = new StringBuilder(128);
            public readonly HashSet<Assembly> Assemblies = new HashSet<Assembly>();
        }

        sealed class ExceptionData
        {
            public readonly Dictionary<string, OwnerData> PatchesByPlugin = new Dictionary<string, OwnerData>(StringComparer.Ordinal);
            public HashSet<MethodBase> PossibleMethods = new HashSet<MethodBase>();
            public string Stacktrace = string.Empty;
            public string Message = string.Empty;
            public int Count = 0;

            public void AddExceptionPatches(string prefix, ReadOnlyCollection<Patch> buckets)
            {
                for (int i = 0; i < buckets.Count; ++i)
                {
                    Patch patch = buckets[i];
                    string owner = patch.owner;

                    if (!PatchesByPlugin.TryGetValue(owner, out OwnerData ownerData))
                    {
                        ownerData = new OwnerData();
                        PatchesByPlugin[owner] = ownerData;
                    }

                    ownerData.Patches
                        .Append(prefix)
                        .Append("[index=")
                        .Append(patch.index)
                        .Append("]: ")
                        .AppendLine(patch.PatchMethod.FullDescription());

                    ownerData.Assemblies.Add(patch.PatchMethod.DeclaringType.Assembly);
                }
            }

            static readonly string _pluginsSegment = Path.DirectorySeparatorChar + "plugins" + Path.DirectorySeparatorChar;

            public void GetClipboardText(StringBuilder sb)
            {
                // Prep it for markdown :)
                sb.Append("Exception hit ").Append(Count).Append(" times: ")
                    .AppendLine(Message)
                    .AppendLine(Stacktrace);

                sb.AppendLine("Target methods matching by name:");
                foreach (MethodBase method in PossibleMethods)
                {
                    sb.AppendLine(method.FullDescription());
                }

                sb.AppendLine().AppendLine("Relevant plugins:");


                int i = 0;
                foreach (KeyValuePair<string, OwnerData> kvp in PatchesByPlugin)
                {
                    sb.Append(++i)
                      .Append(". ")
                      .Append(kvp.Key)
                      .Append(" - ");

                    foreach (Assembly assembly in kvp.Value.Assemblies)
                    {
                        string assemblyPath = assembly.Location;

                        int relativePathIndex = assemblyPath.LastIndexOf(_pluginsSegment);
                        if (relativePathIndex >= 0)
                        {
                            int start = relativePathIndex + _pluginsSegment.Length;
                            sb.Append(assemblyPath, start, assemblyPath.Length - start);
                        }
                    }

                    sb.AppendLine()
                      .Append('\t')
                      .AppendLine(kvp.Value.Patches.ToString());
                }
            }
        }

        void AddExceptionData(string message, string target, MethodBase result)
        {
            if (_exceptionMap.TryGetValue(target, out ExceptionData exceptionData))
            {
                exceptionData.PossibleMethods.Add(result);
                exceptionData.Count++;
                return;
            }

            exceptionData = new ExceptionData {
                Stacktrace = target,
                Message = message,
                Count = 1,
            };

            Patches patchInfo = PatchProcessor.GetPatchInfo(result);
            exceptionData.PossibleMethods.Add(result);
            exceptionData.AddExceptionPatches("Finalizer", patchInfo.Finalizers);
            exceptionData.AddExceptionPatches("Prefix", patchInfo.Postfixes);
            exceptionData.AddExceptionPatches("Postfix", patchInfo.Prefixes);
            exceptionData.AddExceptionPatches("Transpiler", patchInfo.Transpilers);

            _exceptionMap[target] = exceptionData;
        }


        readonly StacktraceParser _parser = new StacktraceParser();
        UIButton _btn;

        void OnLogReceived(string errorString, string stackTrace, LogType logType)
        {
            if (logType != LogType.Error && logType != LogType.Exception)
            {
                return;
            }

            // Prevent this from running if there's no fatal error tip window.
            if (UIFatalErrorTip.instance == null)
            {
                return;
            }

            if (_patchMap == null)
            {
                _patchMap = new Dictionary<string, List<MethodBase>>();

                foreach (MethodBase patchedMethod in PatchProcessor.GetAllPatchedMethods())
                {
                    string key = patchedMethod.DeclaringType.FullName;
                    if (!_patchMap.TryGetValue(key, out List<MethodBase> methods))
                    {
                        methods = new List<MethodBase>();
                        _patchMap[key] = methods;
                    }

                    methods.Add(patchedMethod);
                }
            }

            // The devs log with LogError(exception);
            if (string.IsNullOrEmpty(stackTrace)) {
                int end = errorString.IndexOf('\n');
                if (end != -1)
                {
                    stackTrace = errorString.Substring(end);
                    errorString = errorString.Substring(0, end);
                }
            }

            bool update = _parser.ParseStackTraceLines(stackTrace, (type, method) => {
                if (_patchMap.TryGetValue(type, out List<MethodBase> classPatched))
                {
                    foreach (MethodBase methodBase in classPatched)
                    {
                        if (methodBase.Name == method)
                        {
                            // TODO: Check parameters.
                            AddExceptionData(errorString, stackTrace, methodBase);
                        }
                    }
                }
            });

            if (update)
            {
                UIFatalErrorTip.instance.ShowError(errorString, stackTrace);

                if (_btn == null)
                {
                    try
                    {
                        var go = GameObject.Find("UI Root/Overlay Canvas/Top Windows/Option Window/apply-button");
                        var rect = (RectTransform)Instantiate(go).transform;

                        rect.anchorMax = new Vector2(1, 0);
                        rect.anchorMin = new Vector2(1, 0);
                        rect.sizeDelta = new Vector2(120, 34);
                        rect.pivot = new Vector2(1, 0);
                        rect.anchoredPosition = new Vector2(-2, 2);

                        rect.SetParent(UIFatalErrorTip.instance.transform, false);

                        _btn = rect.gameObject.GetComponent<UIButton>();
                        _btn.onClick += OnClick;

                        // panel is 0.311, 0, 0.001, 0.902
                        Image image = _btn.GetComponent<Image>();
                        if (image != null)
                        {
                            image.color = new Color(0.8f, 0.1f, 0.1f, 1);
                        }

                        Text text = _btn.GetComponentInChildren<Text>();
                        if (text != null)
                        {
                            text.text = "Copy to Clipboard";
                        }
                    }
                    catch (Exception e)
                    {
                        gameObject.SetActive(false);
                        Logger.LogWarning("Could not create copy button, disabling plugin: " + e.ToString());
                    }
                }
            }
        }

        void OnClick(int obj)
        {
            var sb = new StringBuilder();

            sb.AppendLine("```");
            foreach (KeyValuePair<string, ExceptionData> e in _exceptionMap)
            {
                e.Value.GetClipboardText(sb);
                sb.AppendLine("==================");
            }
            sb.AppendLine("```");

            GUIUtility.systemCopyBuffer = sb.ToString();
        }

        void OnDisable()
        {
            Application.logMessageReceived -= OnLogReceived;
        }
    }
}
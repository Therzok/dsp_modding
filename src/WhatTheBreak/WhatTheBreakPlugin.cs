using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using MonoMod.Utils;

using UnityEngine;

namespace WhatTheBreak
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public sealed class WhatTheBreakPlugin : BaseUnityPlugin
    {
        readonly Harmony _harmony = new Harmony(ThisAssembly.Plugin.HarmonyGUID);
        readonly Dictionary<string, ExceptionData> _exceptionMap = new Dictionary<string, ExceptionData>(StringComparer.Ordinal);

        Dictionary<string, List<MethodBase>> _patchMap = null;

        void Awake()
        {
            XLogHandler.onFatalError += XLogHandler_onException;
            XLogHandler.onException += XLogHandler_onException;
            MyPatches.OnException += OnPatchException;
            _harmony.PatchAllRecursive(typeof(MyPatches));

            Logger.LogInfo("Loaded " + nameof(WhatTheBreakPlugin));
        }

        void OnPatchException(Exception e)
        {
            Logger.LogInfo(e.ToString());
            Logger.LogInfo("YAY");
        }

        sealed class ExceptionData
        {
            public List<string> Patchers = new List<string>();
            public string Message;
            public string Stacktrace;
            public int Count;
        }

        const string DMDHeader = "(wrapper dynamic-method) ";

        readonly char [] whitespace= { '\t', ' ' };

        bool ParseStackTraceLines(string source)
        {
            bool foundPatcher = false;

            foreach (string original in source.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string line = original;
                if (!line.StartsWith(DMDHeader))
                {
                    // Not interesting.
                    continue;
                }

                // type and method
                int start = DMDHeader.Length;
                int typeAndMethodEnd = line.IndexOfAny(whitespace, start);

                //string typeAndMethod = line.Substring(start, typeAndMethodEnd - start);
                //Logger.LogInfo("tam: " + typeAndMethod);

                int typeEnd = line.LastIndexOf(".DMD<", start, typeAndMethodEnd);

                //string type = line.Substring(start, typeEnd - start);
                string patchInfo = line.Substring(typeEnd + ".DMD<".Length, typeAndMethodEnd - start - 1);

                Logger.LogInfo(patchInfo);
                int patchSep = patchInfo.IndexOf("..");
                string patchType = patchInfo.Substring(0, patchSep);
                string patchMethod = patchInfo.Substring(patchSep + 2);

                Logger.LogInfo(patchType);
                Logger.LogInfo(patchMethod);

                if (_patchMap.TryGetValue(patchType, out var methods))
                {
                    foreach (var method in methods)
                    {
                        if (method.Name == patchMethod)
                        {

                        }
                    }
                }

                // Skip over it.
                start = typeAndMethodEnd + 1;

                int end = line.IndexOfAny(whitespace, start);
                if (end != -1 && line[end] == ')' && line[start] == '(')
                {
                    end--; start++;

                    string parameters = line.Substring(start, end - start);
                    Logger.LogInfo("par: " + parameters);
                }

                // parse parameters.

            }

            return foundPatcher;
        }

        void XLogHandler_onException(string errorString, string stackTrace)
        {
            if (_patchMap == null)
            {
                _patchMap = new Dictionary<string, List<MethodBase>>();
                foreach (var patchedMethod in PatchProcessor.GetAllPatchedMethods())
                {
                    var key = patchedMethod.DeclaringType.FullName;
                    if (!_patchMap.TryGetValue(key, out var methods))
                    {
                        methods = new List<MethodBase>();
                        _patchMap[key] = methods;
                    }

                    methods.Add(patchedMethod);
                }
            }


            if (string.IsNullOrEmpty(stackTrace))
            {
                // Logged by the devs, they do LogError(exception);
                // 
                ParseStackTraceLines(errorString);
            }
            else
            {
                ParseStackTraceLines(stackTrace);
            }

                // at (wrapper dynamic-method) ContainingType.DMD<ClassPatched..MethodPatched> (ClassPatched)
                // at (wrapper dynamic-method) UILoadGameWindow.DMD<UILoadGameWindow..RefreshList> (UILoadGameWindow) <0x0033b>
                // Test+Generic`1[T].ThrowMe

        }

        void OnDestroy()
        {
            XLogHandler.onFatalError -= XLogHandler_onException;
            XLogHandler.onException -= XLogHandler_onException;

            _harmony.UnpatchSelf();
        }

        class MyPatches
        {
            public static event Action<Exception> OnException;

            [HarmonyPostfix, HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogError), typeof(object))]
            public static void LogError(object message)
            {
                if (message is Exception e)
                {
                    OnException?.Invoke(e);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogError), typeof(object), typeof(UnityEngine.Object))]
            public static void LogError(object message, UnityEngine.Object context)
            {
                if (message is Exception e)
                {
                    OnException?.Invoke(e);
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogException), typeof(Exception))]
            public static void LogException(Exception exception)
            {
                OnException?.Invoke(exception);
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UnityEngine.Debug), nameof(UnityEngine.Debug.LogException), typeof(Exception), typeof(UnityEngine.Object))]
            public static void LogException(Exception exception, UnityEngine.Object context)
            {
                OnException?.Invoke(exception);
            }
        }
    }
}
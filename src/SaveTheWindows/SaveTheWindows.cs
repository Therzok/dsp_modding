using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;

using UnityEngine;

namespace SaveTheWindows
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public sealed class SaveTheWindowsPlugin : BaseUnityPlugin
    {
        readonly FieldInfo _fileInfoAccessor = typeof(UIGameSaveEntry).GetField("fileInfo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
        readonly Harmony _harmony = new Harmony(ThisAssembly.Plugin.HarmonyGUID);

        void Awake()
        {
            _harmony.PatchAllRecursive(typeof(Patches));

            Logger.LogInfo("Loaded " + nameof(SaveTheWindowsPlugin));
        }

        void OnEnable()
        {
            Patches.Refreshed += OnListRefreshed;
            Logger.LogInfo("Patches hooked");
        }

        void OnDisable()
        {
            Patches.Refreshed -= OnListRefreshed;
            Logger.LogInfo("Patches unhooked");
        }

        void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        void OnListRefreshed(List<UIGameSaveEntry> entries)
        {
#if VERBOSE_LOG
            using (Logger.DevMeasure("SORT", "sorting save entries"))
            {
#endif
            // Sort the save list, descending order.
            entries.Sort((x, y) => y.fileDate.CompareTo(x.fileDate));

            for (int i = 0; i < entries.Count; ++i)
            {
                UIGameSaveEntry entry = entries[i];
                var fileInfo = (FileInfo)_fileInfoAccessor.GetValue(entry);

                // Trigger update of UI entities
                entry.SetEntry(i, fileInfo);

                Logger.DevLogInfo(string.Format("{0} {1} {2}", i, entry.saveName, fileInfo.LastWriteTime));
            }
#if VERBOSE_LOG
            }
#endif
        }

        static class Patches
        {
            public static Action<List<UIGameSaveEntry>> Refreshed;

            // Requesting the data on window open/close might be expensiv
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ManualBehaviour), nameof(ManualBehaviour._Create))]
            [HarmonyPatch(typeof(ManualBehaviour), nameof(ManualBehaviour._Destroy))]
            [HarmonyPatch(typeof(UILoadGameWindow), nameof(UILoadGameWindow.RefreshList))]
            public static void RefreshList(List<UIGameSaveEntry> ___entries)
            {
                Refreshed?.Invoke(___entries);
            }
        }
    }
}

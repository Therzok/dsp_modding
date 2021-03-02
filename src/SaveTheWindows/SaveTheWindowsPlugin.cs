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
        readonly Harmony _harmony = new Harmony(ThisAssembly.Plugin.HarmonyGUID);
        WindowManager _manager;

#if DEVELOPMENT
        /// <summary>
        /// Forces the windowing system to be serialized.
        /// </summary>
        public void ForceSerialize()
        {
            _manager.SaveData(GameSave.LastExit);
        }
#endif

        void Awake()
        {
            var description = new ConfigDescription("Adds indentation to the save file allowing it to be read by humans. Enabling impacts performance.");
            var humanReadable = Config.Bind(nameof(SaveTheWindows), key: "HumanReadable", defaultValue: false, description);

            _manager = new WindowManager(Path.Combine(GameConfig.gameSaveFolder, nameof(SaveTheWindowsPlugin)), new WindowSerializer(humanReadable));

            _harmony.PatchAllRecursive(typeof(Patches));

            Logger.DevLogInfo("Loaded " + nameof(SaveTheWindowsPlugin));
        }

        void OnEnable()
        {
            Patches.GameSaved += _manager.SaveData;
            Logger.DevLogInfo("Patches hooked");
        }

        void Start()
        {
            // HACK: Maybe listen to UIWindowDrag.OnEnable? Maybe just keep a pair of transform/name?
            var windowRoot = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows");
            var windows = windowRoot.GetComponentsInChildren<UIWindowDrag>(includeInactive: true);

            // TODO: Whenever they release the UIWindow API, consider fixing this and figuring out how to detect canvas of other plugins
            // _draggableWindows = windowRoot.GetComponentsInChildren<UIWindow>(includeInactive: true);

            _manager.RegisterWindows(nameof(DSPGame), Array.ConvertAll(windows, window => window.dragTrans));
        }

        void OnDisable()
        {
            Patches.GameSaved -= _manager.SaveData;
            Logger.DevLogInfo("Patches unhooked");
        }

        void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        static class Patches
        {
            /// <summary>
            /// Triggered when a game is saved (autosave, last exit or manual)
            /// </summary>
            public static Action<string> GameSaved;

            /// <summary>
            /// Triggered when a game save is loaded.
            /// </summary>
            /// <param name="saveName">The name of the save game.</param>
            [HarmonyPostfix, HarmonyPatch(typeof(GameSave), nameof(GameSave.SaveCurrentGame))]
            public static void SaveCurrentGame(string saveName)
            {
                GameSaved?.Invoke(saveName);
            }

            //[HarmonyPostfix, HarmonyPatch(typeof(ManualBehaviour), nameof(ManualBehaviour._Destroy))]
            //public static void Close(ManualBehaviour __instance)
            //{
            //    GameLoaded?.Invoke(__instance);
            //}
        }
    }
}

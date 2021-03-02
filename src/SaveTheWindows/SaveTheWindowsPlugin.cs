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
        readonly WindowSerializer _serializer;
        RectTransform[] _transformsArray = ArrayUtil.Empty<RectTransform>();
        readonly Dictionary<string, RectTransform> _transforms = new Dictionary<string, RectTransform>(64, StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the plugin.
        /// </summary>
        public SaveTheWindowsPlugin()
        {
            _serializer = new WindowSerializer(Config);
        }

        void Awake()
        {
            _harmony.PatchAllRecursive(typeof(Patches));
        }

        void OnEnable()
        {
            Patches.UIOpened += OpenGameUI;
            Patches.UIClosed += CloseGameUI;

            Logger.DevLog();
        }

        const string UISource = "UI Root/Overlay Canvas/In Game/Windows";
        void OpenGameUI()
        {
            long token = 0;

            Logger.MeasureStart(ref token);
            // HACK: Maybe listen to UIWindowDrag.OnEnable? Maybe just keep a pair of transform/name?
            var windowRoot = GameObject.Find(UISource);
            if (windowRoot == null)
            {
                return;
            }

            UIWindowDrag[] windows = windowRoot.GetComponentsInChildren<UIWindowDrag>(includeInactive: true);

            _transformsArray = Array.ConvertAll(windows, window => window.dragTrans);
            foreach (UIWindowDrag wnd in windows)
            {
                _transforms[wnd.name] = wnd.dragTrans;
            }

            // TODO: Whenever they release the UIWindow API, consider fixing this
            // _draggableWindows = windowRoot.GetComponentsInChildren<UIWindow>(includeInactive: true);

            WindowManager.Instance.LoadData(_serializer, nameof(DSPGame), _transforms);

            Logger.MeasureEnd(token);
        }

        void CloseGameUI()
        {
            WindowManager.Instance.SaveData(_serializer, nameof(DSPGame), _transformsArray);
        }

        void OnDisable()
        {
            Patches.UIOpened -= OpenGameUI;
            Patches.UIClosed -= CloseGameUI;

            Logger.DevLog();
        }

        void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        static class Patches
        {
            public static Action UIOpened;

            public static Action UIClosed;

            [HarmonyPostfix, HarmonyPatch(typeof(UIGame), nameof(UIGame.OnGameEnd))]
            public static void OnGameEnd()
            {
                UIClosed?.Invoke();
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIRoot), nameof(UIRoot.OpenGameUI))]
            public static void OpenGameUI()
            {
                UIOpened?.Invoke();
            }
        }
    }
}

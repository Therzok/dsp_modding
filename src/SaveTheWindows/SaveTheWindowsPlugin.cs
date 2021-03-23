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
        readonly Harmony _harmony = new Harmony(ThisAssembly.Plugin.GUID);
        readonly WindowCollector _collector = new WindowCollector();
        WindowManager _manager;

        readonly WindowSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the plugin.
        /// </summary>
        public SaveTheWindowsPlugin()
        {
            _serializer = new WindowSerializer(Logger);
        }

        void Awake()
        {
            _manager = new WindowManager(Path.Combine(GameConfig.gameDocumentFolder, nameof(SaveTheWindowsPlugin)), "ui.dat");
            _harmony.PatchAllRecursive(typeof(Patches));
        }

        void OnEnable()
        {
            Patches.UIOpened += OpenGameUI;
            Patches.UIClosed += CloseGameUI;

            Logger.DevLog();
        }

        void OpenGameUI()
        {
            long token = 0;

            Logger.DevMeasureStart(ref token);

            // This doesn't handle destroyed windows or other scenarios like runtime unload of plugins.
            _manager.LoadData(_serializer, nameof(DSPGame), _collector.GetWindowMap());

            Logger.DevMeasureEnd(token);
        }

        void CloseGameUI()
        {
            _manager.SaveData(_serializer, nameof(DSPGame), _collector.GetSortedWindows());
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

        sealed class WindowCollector
        {
            // Re-use collections where possible
            readonly List<UIWindowDrag> _windows = new List<UIWindowDrag>();
            RectTransform[] _transformsArray = ArrayUtil.Empty<RectTransform>();
            readonly Dictionary<string, RectTransform> _transforms = new Dictionary<string, RectTransform>(64, StringComparer.Ordinal);

            public Dictionary<string, RectTransform> GetWindowMap()
            {
                CollectWindows();

                _transforms.Clear();

                foreach (UIWindowDrag window in _windows)
                {
                    _transforms[window.name] = window.dragTrans;
                }

                _windows.Clear();

                return _transforms;
            }

            public RectTransform[] GetSortedWindows()
            {
                CollectWindows();

                // If we can't reuse the array, allocate a new one.
                if (_transformsArray.Length != _windows.Count)
                {
                    _transformsArray = new RectTransform[_windows.Count];
                }

                for (int i = 0; i < _windows.Count; i++)
                {
                    _transformsArray[i] = _windows[i].dragTrans;
                }

                _windows.Clear();

                Array.Sort(_transformsArray, (x, y) => StringComparer.Ordinal.Compare(x.name, y.name));

                return _transformsArray;
            }

            void CollectWindows()
            {
                // Unhandled classes:
                // UIWindow where canDrag = true

                Transform windows = UIRoot.instance.overlayCanvas.transform.Find("In Game/Windows");
                if (windows != null)
                {
                    windows.GetComponentsInChildren(true, _windows);
                }
            }
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

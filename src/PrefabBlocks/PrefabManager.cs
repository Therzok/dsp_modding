using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using BepInEx.Logging;

using TMPro;

using UnityEngine;

namespace PrefabBlocks
{
    /// <summary>
    /// This class exposes some basic unity primitives that have the default properties and components already set by the unity editor.
    /// </summary>
    public sealed class PrefabManager
    {
        public static readonly PrefabManager Instance = new PrefabManager("PrefabBlocks.prefabblocks");

        readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource(nameof(PrefabManager));
        readonly string _bundleResourceName;
        bool _initialized;

        public GameObject WorldText;
        public GameObject WorldPanel;
        public GameObject UIText;

        AssetBundle _assets;

        PrefabManager(string bundleResourceName)
        {
            _bundleResourceName = bundleResourceName;
        }

        internal void Preload()
        {
            using (Stream stream = typeof(PrefabManager).Assembly.GetManifestResourceStream(_bundleResourceName))
            {
                _assets = AssetBundle.LoadFromStream(stream);
            }
        }

        internal void Unload()
        {
            _initialized = false;
            _assets.Unload(false);
        }

        internal void Destroy()
        {
            _assets.Unload(true);
        }

        internal void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            // Force load all assets, because we need to fix TextMeshPro initialization.
            _assets.LoadAllAssets();

            _initialized = true;

            FixTextMeshProMissing();

            WorldText = _assets.LoadAsset<GameObject>("Assets/worldtext.prefab");
            WorldPanel = _assets.LoadAsset<GameObject>("Assets/worldpanel.prefab");
            UIText = _assets.LoadAsset<GameObject>("Assets/uitext.prefab");
        }

        void FixTextMeshProMissing()
        {
            const string TMPPrefix = "Assets/textmesh pro/resources";

            // Check if we have a default style sheet set, if we do, we're fine.
            TMP_StyleSheet tryOriginal = Resources.Load<TMP_StyleSheet>("Style Sheets/Default Style Sheet"); ;
            if (tryOriginal != null)
            {
                // Already present, so no need to do anything.
                return;
            }

            // Set the default settings.
            TMP_Settings settings = _assets.LoadAsset<TMP_Settings>(TMPPrefix + "/tmp settings.asset");
            typeof(TMP_Settings)
                .GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, settings);

            // Set the default stylesheet.
            TMP_StyleSheet styleSheet = _assets.LoadAsset<TMP_StyleSheet>(TMPPrefix + "/style sheets /default style sheet.asset");
            typeof(TMP_StyleSheet)
                .GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, styleSheet);
        }
    }
}

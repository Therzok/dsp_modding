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

namespace PrefabBlocks
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public sealed class PrefabBlocksPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            // Trigger loading the asset bundle
            PrefabManager.Instance.Preload();
        }

        void OnEnable()
        {
            PrefabManager.Instance.Initialize();
            Logger.LogMessage("Prefablocks assets loaded and fixed TextMeshPro");
        }

        void OnDisable()
        {
            PrefabManager.Instance.Unload();
        }

        void OnDestroy()
        {
            PrefabManager.Instance.Destroy();
        }
    }
}

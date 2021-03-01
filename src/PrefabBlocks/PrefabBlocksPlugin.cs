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
            Logger.DevLogInfo("Asset bundles loaded");
        }

        void OnEnable()
        {
            PrefabManager.Instance.Initialize();
            Logger.DevLogInfo("Requested prefabs and fixed TextMeshPro");
        }

        void OnDisable()
        {
            PrefabManager.Instance.Unload();
            Logger.DevLogInfo("Soft unloaded and deactivated");
        }

        void OnDestroy()
        {
            PrefabManager.Instance.Destroy();

            Logger.DevLogInfo("Destroyed");
        }
    }
}

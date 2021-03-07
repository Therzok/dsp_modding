using System;
using System.Collections;
using System.Collections.Generic;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using TMPro;

using UnityEngine;

namespace YouNameIt
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    [BepInDependency(ThisAssembly.Plugin.Authors + "." + nameof(PrefabBlocks))]
    public sealed class YouNameItPlugin : BaseUnityPlugin
    {
        readonly Harmony _harmony = new Harmony(ThisAssembly.Plugin.HarmonyGUID);
        readonly LifetimeTrackerPool _trackerPool;

        /// <summary>
        /// Creates a new Plugin instance.
        /// </summary>
        public YouNameItPlugin()
        {
            _trackerPool = new LifetimeTrackerPool(Config);
        }

        void Awake()
        {
            _harmony.PatchAllRecursive(typeof(Patches));

            Logger.LogInfo("Loaded " + nameof(YouNameItPlugin));
        }

        void OnEnable()
        {
            _trackerPool.Enable();
        }

        void OnDisable()
        {
            _trackerPool.Disable();
        }

        void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
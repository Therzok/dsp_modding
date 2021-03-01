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
    [BepInDependency("org.Therzok.dsp.PrefabBlocks")]
    public sealed class YouNameItPlugin : BaseUnityPlugin
    {
        readonly Harmony _harmony = new Harmony(ThisAssembly.Plugin.HarmonyGUID);
        readonly LifetimeTrackerPool _trackerPool = new LifetimeTrackerPool();

        void Awake()
        {
            _harmony.PatchAllRecursive(typeof(Patches));

            Logger.LogInfo("Loaded " + nameof(YouNameItPlugin));
        }

        void OnEnable()
        {
            Hooks.Game.Begin += _trackerPool.Enable;
            Hooks.Game.End += _trackerPool.Disable;
        }

        void OnDisable()
        {
            Hooks.Game.Begin -= _trackerPool.Enable;
            Hooks.Game.End -= _trackerPool.Disable;
        }

        void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
    }
}
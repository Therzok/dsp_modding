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

namespace DumpAssets
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public sealed class DumpAssetsPlugin : BaseUnityPlugin
    {
        void Awake()
        {
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

        void OnDestroy()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using BepInEx.Logging;

using HarmonyLib;

namespace HarmonyLib
{
    /// <summary>
    /// A collection of extension methods to enable diagnosing easier
    /// </summary>
    static class HarmonyExtensions
    {
        /// <summary>
        /// Recursively goes through a type and its children to apply patches. Useful for structured patch definitions.
        /// </summary>
        /// <param name="harmony">The patching instance</param>
        /// <param name="type">The toplevel type</param>
        public static void PatchAllRecursive(this Harmony harmony, Type type)
        {
            harmony.PatchAllRecursiveInternal(type);

            using (ManualLogSource log = Logger.CreateLogSource("Hrm"))
            {
                log.DevLogInfo(harmony);
            }
        }

        // The recursive variant.
        static void PatchAllRecursiveInternal(this Harmony harmony, Type type)
        {
            foreach (Type child in type.GetNestedTypes())
            {
                harmony.PatchAllRecursiveInternal(child);
            }

            harmony.PatchAll(type);
        }
    }
}

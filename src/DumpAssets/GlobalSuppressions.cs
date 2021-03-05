// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance",
    "HAA0401:Possible allocation of reference type enumerator",
    Justification = "Development logging code",
    Scope = "member",
    Target = "~M:BepInEx.Logging.LogExtensions.DevLog(BepInEx.Logging.ManualLogSource,HarmonyLib.Harmony)")]

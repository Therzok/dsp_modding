using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;

namespace YouNameIt
{
    static class Hooks
    {
        public static class Game
        {
            public static Action Begin;
            public static Action End;
        }

        public static class Factory
        {
            public static Action<PlanetData> Loaded;
            public static Action<PlanetData> Unloading;
        }

        public static class Transport
        {
            public static Action<PlanetTransport, PrefabDesc, StationComponent> Added;

            public static Action<PlanetTransport, int> Removed;
        }

        public static class StationWindow
        {
            public static Action<UIStationWindow, string> Submit;
        }
    }

    static class Patches
    {
        [HarmonyPatch(typeof(GameMain))]
        public class GameMainPatch
        {
            [HarmonyPostfix, HarmonyPatch(nameof(Begin))]
            public static void Begin()
            {
                Hooks.Game.Begin?.Invoke();
            }

            [HarmonyPostfix, HarmonyPatch(nameof(End))]
            public static void End()
            {
                Hooks.Game.End?.Invoke();
            }
        }

        [HarmonyPatch(typeof(UIStationWindow))]
        public class UIStationWindowPatch
        {
            [HarmonyPostfix, HarmonyPatch(nameof(OnNameInputSubmit))]
            public static void OnNameInputSubmit(UIStationWindow __instance, string s)
            {
                Hooks.StationWindow.Submit?.Invoke(__instance, s);
            }
        }

        [HarmonyPatch(typeof(PlanetTransport))]
        public class PlanetTransportPatch
        {
            [HarmonyPostfix, HarmonyPatch(nameof(PlanetTransport.NewStationComponent))]
            public static void AddStationComponent(PlanetTransport __instance, PrefabDesc _desc, StationComponent __result)
            {
                Hooks.Transport.Added?.Invoke(__instance, _desc, __result);
            }

            [HarmonyPostfix, HarmonyPatch(nameof(PlanetTransport.RemoveStationComponent))]
            public static void RemoveStationComponent(PlanetTransport __instance, int id)
            {
                Hooks.Transport.Removed?.Invoke(__instance, id);
            }
        }

        [HarmonyPatch(typeof(PlanetData))]
        public class PlanetDataPatch
        {
            [HarmonyPostfix, HarmonyPatch(nameof(PlanetData.NotifyFactoryLoaded))]
            public static void NotifyFactoryLoaded(PlanetData __instance)
            {
                Hooks.Factory.Loaded?.Invoke(__instance);
            }

            [HarmonyPrefix, HarmonyPatch(nameof(PlanetData.UnloadFactory))]
            public static void UnloadFactory(PlanetData __instance)
            {
                Hooks.Factory.Unloading?.Invoke(__instance);
            }
        }
    }
}

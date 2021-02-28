using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using HarmonyLib;

namespace BepInEx.Logging
{
    static class LogExtensions
    {

        [Conditional("VERBOSE_LOG")]
        public static void DevLogInfo(this ManualLogSource log, string message)
        {
            log.LogInfo(message);
        }

        [Conditional("VERBOSE_LOG")]
        public static void DevLog(this ManualLogSource log, LogLevel level, string message)
        {
            log.Log(level, message);
        }

#if VERBOSE_LOG
        public static Measure DevMeasure(this ManualLogSource log, string ctx, string message)
        {
            return new Measure(log, ctx, message);
        }

        public readonly struct Measure : IDisposable
        {
            readonly ManualLogSource _log;
            readonly string _ctx;
            readonly long _start;

            public Measure(ManualLogSource log, string ctx, string message)
            {
                _log = log;
                _ctx = ctx;

                log.LogInfo("[" + ctx + "] start: " + message);
                _start = Stopwatch.GetTimestamp();
            }

            public void Dispose()
            {
                long elapsed = (Stopwatch.GetTimestamp() - _start);
                double scale = (double)Stopwatch.Frequency / TimeSpan.TicksPerSecond;

                _log.LogInfo("[" + _ctx + " ] end " + TimeSpan.FromTicks((long)(elapsed / scale)).ToString());
            }
        }
#endif

        [Conditional("VERBOSE_LOG")]
        public static void DevLogInfo(this ManualLogSource log, Harmony harmony)
        {
            foreach (MethodBase method in harmony.GetPatchedMethods())
            {
                Patches patches = Harmony.GetPatchInfo(method);
                log.DevLogInfo(string.Format("{0}::{1} patchers:", method.DeclaringType.FullName, method.Name));

                log.DevLogInfo(string.Join(" ", patches.Owners.ToArray()));
                LogPatchSegment(log, method, patches.Prefixes, "Prefixes:");
                LogPatchSegment(log, method, patches.Transpilers, "Transpilers:");
                LogPatchSegment(log, method, patches.Postfixes, "Postfixes:");
                LogPatchSegment(log, method, patches.Finalizers, "Postfixes:");
            }
        }

        [Conditional("VERBOSE_LOG")]
        static void LogPatchSegment(ManualLogSource log, MethodBase original, ReadOnlyCollection<Patch> patches, string header)
        {
            if (patches.Count == 0)
            {
                return;
            }

            log.DevLogInfo(header);
            for (int i = 0; i < patches.Count; ++i)
            {
                Patch patch = patches[i];

                log.DevLogInfo(patch.GetMethod(original).FullDescription());
                log.DevLogInfo(string.Format("[{0}] {1} {2}", patch.owner, patch.index, patch.priority));
            }
        }
    }
}

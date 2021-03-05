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
        int goCount = 0;
        int componentCount = 0;

        void Start()
        {
            string resultDir = Path.Combine(Path.GetDirectoryName(typeof(DumpAssetsPlugin).Assembly.Location), "PrefabsMarkdown");
            Directory.CreateDirectory(resultDir);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            Logger.LogInfo("Processing prefabs, game might freeze...");

            foreach (var gameObject in Resources.LoadAll<GameObject>(""))
            {
                ProcessGameObject("/", resultDir, gameObject);
            }

            sw.Stop();

            Logger.LogInfo(string.Format("Wrote {0} gameobjects {1} components in {2}", goCount, componentCount, sw.Elapsed));
        }

        void ProcessGameObject(string resourcePath, string outputPath, GameObject go)
        {
            string outputBase = Path.Combine(outputPath, go.name);
            string fileName = outputBase + ".md";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                // Title
                writer.WriteHeader(go.name);
                writer.WriteLine();

                // Path
                writer.Write("Path: ");
                writer.Write(resourcePath);
                writer.Write(go.name);

                // Tag
                if (!string.IsNullOrEmpty(go.tag))
                {
                    writer.Write("Tag: ");
                    writer.WriteLine(go.tag);
                }

                go.GetComponents(components);

                foreach (var component in components)
                {
                    if (!cachedFields.TryGetValue(component.GetType(), out var fields))
                    {
                        //
                    }
                }

                // child -> resourcePath + "/", outputBase

                // Components
                // Children
            }
        }

        List<Component> components = new List<Component>();
        Dictionary<Type, FieldInfo[]> cachedFields = new Dictionary<Type, FieldInfo[]>();

        class FastPath
        {
            public char[] Buffer;
            public int Count;
            char sep;

            public FastPath(char sep)
            {
                Buffer = new char[1024];
                Count = 0;
            }

            public int Push(string component)
            {
                Buffer[Count++] = sep;

                if (Buffer.Length - Count < component.Length)
                {
                    Array.Resize(ref Buffer, Buffer.Length * 2);
                }

                component.CopyTo(0, Buffer, Count, component.Length);

                return component.Length + 1;
            }

            public void Pop(int count)
            {
                Count -= count;
                Array.Clear(Buffer, Count, count);
            }
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

    static class StreamWriterExtensions
    {
        public static void WriteHeader(this StreamWriter writer, string text)
        {
            writer.Write("# ");
            writer.WriteLine(text);
        }

        public static void WriteHeader2(this StreamWriter writer, string text)
        {
            writer.Write("## ");
            writer.WriteLine(text);
        }
    }
}

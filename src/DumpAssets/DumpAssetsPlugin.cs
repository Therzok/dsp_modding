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
using System.Collections;

namespace DumpAssets
{
    /// <summary>
    /// Main plugin entry point.
    /// </summary>
    [BepInPlugin(ThisAssembly.Plugin.GUID, ThisAssembly.Plugin.Name, ThisAssembly.Plugin.Version)]
    public sealed class DumpAssetsPlugin : BaseUnityPlugin
    {
        const string VersionFile = "VERSION";
        const string ReadmeFile = "README.md";
        const string AssetsDirectory = "assets";
        const string DBVersion = "1";

        readonly Counters _counters = new Counters();
        readonly ConfigEntry<string> _outputPath;

        /// <summary>
        /// Creates a new plugin instance, called by mod loader.
        /// </summary>
        public DumpAssetsPlugin()
        {
            _outputPath = Config.Bind("DumpAssets", "Location", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                new ConfigDescription("The path to create the asset folder dump in, should be a path on disk like " + 
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
        }

        void Start()
        {
            try
            {
                string destinationDir = Initialize(out bool skip, out string versionFile);

                if (skip)
                {
                    Logger.LogInfo("Data is up-to-date, skipping...");
                    return;
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                Logger.LogInfo("Processing prefabs '" + destinationDir + "', game might freeze...");

                foreach (GameObject gameObject in Resources.LoadAll<GameObject>(""))
                {
                    ProcessGameObject("", destinationDir, gameObject);
                }

                sw.Stop();

                Logger.LogInfo("Finished dumping in " + sw.Elapsed.ToString());
                Logger.LogInfo(_counters.GameObject.ToString() + " gameobjects");
                Logger.LogInfo(_counters.Component.ToString() + " components");
                Logger.LogInfo(_counters.Field.ToString() + " fields");
                Logger.LogInfo(_counters.Property.ToString() + " properties");

                string[] contents = new[] { DBVersion, GameConfig.gameVersion.ToString() };
                Logger.LogInfo("Version " + DBVersion);
                Logger.LogInfo(contents[1]);

                File.WriteAllLines(Path.Combine(destinationDir, VersionFile), contents);
            }
            catch (Exception e)
            {
                Logger.LogError("Could not process gameobjects, stopping..." + e.ToString());
            }
        }

        string Initialize(out bool skip, out string versionFile)
        {
            string destinationDir = _outputPath.Value;
            if (!Path.IsPathRooted(destinationDir))
            {
                destinationDir = Path.Combine(Path.GetDirectoryName(typeof(DumpAssetsPlugin).Assembly.Location), destinationDir);
            }
            // Resolve relative dirs.
            destinationDir = Path.GetFullPath(destinationDir);

            versionFile = Path.Combine(destinationDir, VersionFile);
            string helpFile = Path.Combine(destinationDir, ReadmeFile);

            destinationDir = Path.Combine(destinationDir, AssetsDirectory);
            Directory.CreateDirectory(destinationDir);

            skip = false;
            try
            {
                string content = File.ReadAllText(versionFile);
                string[] parts = content.Split(new[] { '\n' });
                if (parts.Length == 2 && parts[0] == DBVersion && parts[1] == GameConfig.gameVersion.ToString())
                {
                    skip = true;
                }
            }
            catch
            {
            }

            File.WriteAllText(helpFile, @"Generated via DumpAssetsPlugin from https://github.com/Therzok/dsp_modding");

            return destinationDir;
        }

        readonly char[] _invalidChars = Path.GetInvalidFileNameChars();
        string Sanitize(string name)
        {
            if (name.IndexOfAny(_invalidChars) != -1)
            {
                foreach (char ch in _invalidChars)
                {
                    name = name.Replace(ch, '_');
                }
            }

            return name;
        }

        void ProcessGameObject(string resourcePath, string outputPath, GameObject go)
        {
            if (!_visited.Add(go))
            {
                return;
            }

            _counters.GameObject++;

            string sanitized = Sanitize(go.name);
            string outputBase = Path.Combine(outputPath, sanitized);

            int childCount = go.transform.childCount;

            string fileName = outputBase + ".md";
            using (var writer = new StreamWriter(fileName))
            {
                // Title
                writer.WriteHeader(go.name);

                // Path
                writer.WriteKeyValue("Path", resourcePath, go.name);

                // Tag
                if (!string.IsNullOrEmpty(go.tag))
                {
                    writer.WriteKeyValue("Tag", go.tag);
                }

                var components = new List<Component>(8);
                go.GetComponents(components);

                if (components.Count > 0)
                {
                    writer.WriteHeader2("Components");

                    writer.Write("Count: ");
                    writer.WriteLine(components.Count);
                    writer.WriteLine();

                    writer.WriteLine("```");

                    for (int i = 0; i < components.Count; ++i)
                    {
                        ProcessComponent(writer, components[i]);
                    }
                    writer.WriteLine("```");
                }

                if (childCount > 0)
                {
                    writer.WriteLine();
                    writer.WriteHeader2("Children");
                }

                for (int i = 0; i < childCount; ++i)
                {
                    writer.WriteChildLink(sanitized, Sanitize(go.transform.GetChild(i).name));
                }
            }

            if (childCount > 0)
            {
                Directory.CreateDirectory(outputBase);
                resourcePath = resourcePath + "/" + go.name;

                for (int i = 0; i < childCount; ++i)
                {
                    ProcessGameObject(resourcePath, outputBase, go.transform.GetChild(i).gameObject);
                }
            }
        }

        readonly HashSet<object> _visited = new HashSet<object>();
        readonly Dictionary<Type, ReflectionInfo> _cachedFields = new Dictionary<Type, ReflectionInfo>(new TypeEqualityComparer());
        struct ReflectionInfo
        {
            public FieldInfo[] Fields;
            public PropertyInfo[] Properties;
        }

        void ProcessComponent(StreamWriter writer, Component value)
        {
            if (!_visited.Add(value))
            {
                return;
            }

            _counters.Component++;

            Type type = value.GetType();
            if (!_cachedFields.TryGetValue(type, out ReflectionInfo info))
            {
                info = new ReflectionInfo {
                    Fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                    Properties = value is Transform
                        ? type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                              .Where(x => x.CanRead && x.PropertyType.IsValueType && x.GetIndexParameters().Length == 0)
                              .ToArray()
                        : ArrayUtil.Empty<PropertyInfo>(),
                };

                _cachedFields[type] = info;
            }

            writer.WriteLine(type.FullName);

            foreach (FieldInfo field in info.Fields)
            {
                _counters.Field++;

                writer.WriteField(field);

                object fieldValue = field.GetValue(value);
                ProcessValue(writer, fieldValue);
            }

            foreach (PropertyInfo prop in info.Properties)
            {
                _counters.Property++;

                writer.WriteProperty(prop);

                object propValue = prop.GetValue(value, null);
                ProcessValue(writer, propValue);
            }
        }

        void ProcessValue(StreamWriter writer, object value)
        {
            if (value is IEnumerable enumerable)
            {
                if (value is string str)
                {
                    writer.Write('"');
                    writer.Write(str);
                    writer.WriteLine('"');
                }
                if (enumerable is ICollection collection && collection.Count == 0)
                {
                    writer.WriteLine("{}");
                    return;
                }

                writer.WriteLine();
                writer.WriteLine("{");

                foreach (object child in enumerable)
                {
                    writer.Write('\t');
                    ProcessValue(writer, child);
                }
                writer.WriteLine("},");
            }
            else if (value is Component child)
            {
                ProcessComponent(writer, child);
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        sealed class Counters
        {
            public int GameObject;
            public int Component;
            public int Field;
            public int Property;
        }
    }

    static class StreamWriterExtensions
    {
        public static void WriteValue(this StreamWriter writer, object objectValue)
        {
            // Handle recursion + collections
            writer.WriteLine(objectValue == null ? "null" : objectValue);
        }

        public static void WriteProperty(this StreamWriter writer, PropertyInfo propertyInfo)
        {
            writer.Write(propertyInfo.PropertyType.FullName);
            writer.Write(' ');
            writer.Write(propertyInfo.Name);
            writer.Write(" => ");
        }

        public static void WriteField(this StreamWriter writer, FieldInfo field)
        {
            writer.Write(field.IsPublic ? "public " : "private ");
            if (field.IsStatic)
            {
                writer.Write("static ");
            }

            writer.Write(field.FieldType.FullName);
            writer.Write(' ');
            writer.Write(field.Name);
            writer.Write(" = ");
        }

        public static void WriteKeyValue(this StreamWriter writer, string key, string value)
        {
            writer.Write(key);
            writer.Write(": `");
            writer.Write(value);
            writer.WriteLine("`");
            writer.WriteLine();
        }

        public static void WriteKeyValue(this StreamWriter writer, string key, string valuePrefix, string value)
        {
            writer.Write(key);
            writer.Write(": `");
            writer.Write(valuePrefix);
            writer.Write(value);
            writer.WriteLine("`");
            writer.WriteLine();
        }

        public static void WriteChildLink(this StreamWriter writer, string goName, string childName)
        {
            writer.Write("[");
            writer.Write(childName);
            writer.Write("](");
            writer.Write(Uri.EscapeUriString(goName));
            writer.Write("/");
            writer.Write(Uri.EscapeUriString(childName));
            writer.WriteLine(".md)");
            writer.WriteLine();
        }

        public static void WriteHeader(this StreamWriter writer, string text)
        {
            writer.Write("# ");
            writer.WriteLine(text);
            writer.WriteLine();
        }

        public static void WriteHeader2(this StreamWriter writer, string text)
        {
            writer.Write("## ");
            writer.WriteLine(text);
            writer.WriteLine();
        }
    }

    sealed class TypeEqualityComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return x == y;
        }

        public int GetHashCode(Type obj)
        {
            return obj == null ? 0 : obj.GetHashCode();
        }
    }
}

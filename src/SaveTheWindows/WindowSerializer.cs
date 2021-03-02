﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using BepInEx.Configuration;
using BepInEx.Logging;

using UnityEngine;

namespace SaveTheWindows
{
    sealed class WindowSerializer : IDisposable
    {
        readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource(nameof(WindowManager));

        readonly XmlWriterSettings _xmlWriterSettings;
        readonly XmlReaderSettings _xmlReaderSettings;
        readonly ConfigEntry<bool> _humanReadable;

        public WindowSerializer(ConfigFile config)
        {
            var description = new ConfigDescription("Adds indentation to the save file allowing it to be read by humans. Enabling impacts performance.");
            _humanReadable = config.Bind(nameof(SaveTheWindows), key: "HumanReadable", defaultValue: false, description);

            _humanReadable.SettingChanged += HumanReadableSettingChanged;

            _xmlWriterSettings = new XmlWriterSettings {
                CheckCharacters = false,
                CloseOutput = true,
                Encoding = Encoding.ASCII,
#if VERBOSE_LOG
                Indent = true,
#else
                Indent = _humanReadable.Value,
#endif
            };

            _xmlReaderSettings = new XmlReaderSettings {
                CheckCharacters = false,
                CloseInput = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
            };
        }

        public void Dispose()
        {
            _humanReadable.SettingChanged -= HumanReadableSettingChanged;
        }

        void HumanReadableSettingChanged(object sender, EventArgs e)
        {
            _xmlWriterSettings.Indent = _humanReadable.Value;
        }

        internal bool LoadData(string saveFileName, string source, Dictionary<string, RectTransform> windows)
        {
            using (FileStream fs = File.OpenRead(saveFileName))
            using (var reader = new BinaryReader(fs))
            {
                int version = reader.ReadInt32(); // Version
                if (version != 1 || source != reader.ReadString())
                {
                    _log.LogWarning("Version mismatch, expected '1' got " + source);
                    return false;
                }

                int count = reader.ReadInt32();

                for (int i = 0; i < count; ++i)
                {
                    string name = reader.ReadString();
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();

                    if (windows.TryGetValue(name, out RectTransform window))
                    {
                        window.anchoredPosition.Set(x, y);
                    }
                }
            }

            return true;
        }

        internal void SaveData(string saveFileName, string source, RectTransform[] windows)
        {
            using (FileStream fs = File.OpenWrite(saveFileName))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(1); // Version
                writer.Write(source);

                writer.Write(windows.Length);
                for (int i = 0; i < windows.Length; ++i)
                {
                    RectTransform transform = windows[i];
                    Vector2 anchor = transform.anchoredPosition;
                    writer.Write(transform.name);
                    writer.Write(anchor.x);
                    writer.Write(anchor.y);
                }
            }
        }
    }
}

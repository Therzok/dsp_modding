using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using BepInEx.Configuration;

using UnityEngine;

namespace SaveTheWindows
{
    sealed class WindowSerializer : IDisposable
    {
        readonly XmlWriterSettings _xmlWriterSettings;
        readonly XmlReaderSettings _xmlReaderSettings;
        readonly ConfigEntry<bool> _humanReadable;

        public WindowSerializer(ConfigEntry<bool> humanReadable)
        {
            _humanReadable = humanReadable;
            _humanReadable.SettingChanged += HumanReadableSettingChanged;

            _xmlWriterSettings = new XmlWriterSettings {
                CheckCharacters = false,
                CloseOutput = true,
                Encoding = Encoding.ASCII,
#if VERBOSE_LOG
                Indent = true,
#else
                Indent = humanReadable.Value,
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

        internal void LoadData(string saveFileName)
        {

        }

        internal void SaveData(string saveFileName, List<WindowGroup> windowGroups)
        {
            // TODO: if autosave, shuffle like devs do
            // Or maybe just check for closest to timestamp if autosave :)

            using (var writer = XmlWriter.Create(saveFileName, _xmlWriterSettings))
            {
                writer.WriteComment("Auto-generated, do not modify, ordering is important for performance");

                writer.WriteStartElement(nameof(SaveTheWindowsPlugin));
                writer.WriteAttributeString("Version", "1");

                for (int i = 0; i < windowGroups.Count; ++i)
                {
                    SaveWindowGroup(writer, windowGroups[i]);
                }

                writer.WriteEndElement();
            }
        }

        static void SaveWindowGroup(XmlWriter writer, WindowGroup group)
        {
            writer.WriteStartElement(group.Source);

            var transforms = group.Transforms;
            int length = transforms.Length;
            writer.WriteAttributeString("length", length);

            for (int i = 0; i < transforms.Length; ++i)
            {
                var transform = transforms[i];
                Vector2 anchor = transform.anchoredPosition;

                writer.WriteStartElement("win");

                writer.WriteAttributeString("name", transform.name);
                writer.WriteAttributeString("x", anchor.x);
                writer.WriteAttributeString("y", anchor.y);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}

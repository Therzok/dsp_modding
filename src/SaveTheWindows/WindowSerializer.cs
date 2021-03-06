﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using BepInEx.Configuration;
using BepInEx.Logging;

using UnityEngine;

namespace SaveTheWindows
{
    sealed class WindowSerializer
    {
        readonly ManualLogSource _log;
        const int Version = 2;

        public WindowSerializer(ManualLogSource log)
        {
            _log = log;
        }

        internal bool LoadData(string saveFileName, string source, Dictionary<string, RectTransform> windows)
        {
            long token = 0;

            _log.DevMeasureStart(ref token);

            using (var reader = new BinaryReader(File.OpenRead(saveFileName)))
            {
                int version = reader.ReadInt32(); // Version
                if (version > Version)
                {
                    _log.LogWarning(string.Format("Version mismatch, expected '{0}' got '{1}'", "2", version));
                    return false;
                }

                string currentSource = reader.ReadString();
                if (source != currentSource)
                {
                    _log.LogWarning(string.Format("Version mismatch, expected '{0}' got '{1}'", source, currentSource));
                    return false;
                }

                int count = reader.ReadInt32();

                for (int i = 0; i < count; ++i)
                {
                    string name = reader.ReadString();

                    int x, y;
                    if (version == 1)
                    {
                        x = Mathf.RoundToInt(reader.ReadSingle());
                        y = Mathf.RoundToInt(reader.ReadSingle());
                    }
                    else
                    {
                        x = reader.ReadInt32();
                        y = reader.ReadInt32();
                    }

                    if (windows.TryGetValue(name, out RectTransform window))
                    {
                        window.anchoredPosition = new Vector2(x, y);
                    }
                    else
                    {
                        _log.LogWarning(string.Format("Discarding saved window position '{0}'", name));
                    }
                }
            }

            _log.DevMeasureEnd(token);

            return true;
        }

        internal void SaveData(string saveFileName, string source, RectTransform[] windows)
        {
            long token = 0;

            _log.DevMeasureStart(ref token);

            using (var writer = new BinaryWriter(File.OpenWrite(saveFileName)))
            {
                writer.Write(Version); // Version
                writer.Write(source);

                writer.Write(windows.Length);
                for (int i = 0; i < windows.Length; ++i)
                {
                    RectTransform transform = windows[i];
                    Vector2 anchor = transform.anchoredPosition;
                    writer.Write(transform.name);
                    writer.Write(Mathf.RoundToInt(anchor.x));
                    writer.Write(Mathf.RoundToInt(anchor.y));
                }
            }

            _log.DevMeasureEnd(token);
        }
    }
}

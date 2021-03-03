using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

using BepInEx.Configuration;
using BepInEx.Logging;

using UnityEngine;

namespace SaveTheWindows
{
    /// <summary>
    /// The class to use to register 
    /// </summary>
    public sealed partial class WindowManager
    {
        /// <summary>
        /// The instance of the WindowManager to use.
        /// </summary>
        public static readonly WindowManager Instance = new WindowManager(Path.Combine(GameConfig.gameSaveFolder, nameof(SaveTheWindowsPlugin)));

        readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource(nameof(WindowManager));
        readonly string _saveDirectory;
        readonly string _saveFile;

        WindowManager(string saveDirectory)
        {
            _saveDirectory = saveDirectory;
            _saveFile = Path.Combine(_saveDirectory, "ui.xml");
        }

        internal bool LoadData(WindowSerializer serializer, string source, Dictionary<string, RectTransform> transforms)
        {
            try
            {
                if (File.Exists(_saveFile))
                {
                    return serializer.LoadData(_saveFile, source, transforms);
                }
            }
            catch (Exception e)
            {
                _log.LogError("Failed to load UI data: " + e.ToString());
            }

            return false;
        }

        internal void SaveData(WindowSerializer serializer, string source, RectTransform[] transforms)
        {
            try
            {
                Directory.CreateDirectory(_saveDirectory);

                serializer.SaveData(_saveFile, source, transforms);
            }
            catch (Exception e)
            {
                _log.LogError("Failed to save UI data: " + e.ToString());
            }
        }
    }
}

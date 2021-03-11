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
        readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource(nameof(WindowManager));
        readonly string _saveDirectory;
        readonly string _saveFile;
        readonly string _saveFileTemp;

        internal WindowManager(string saveDirectory, string saveFileName)
        {
            _saveDirectory = saveDirectory.Replace('/', Path.DirectorySeparatorChar);
            _saveFile = Path.Combine(_saveDirectory, saveFileName);
            _saveFileTemp += ".tmp";
        }

        static string GetBackupFileName(string original)
        {
            return original + DateTime.Now.ToString(" yyyy-MM-dd_HH-mm-ss");
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
                _log.LogError(string.Format("Failed to load UI data, moving {0} to backup: {1}", _saveFile, e.ToString()));

                try
                {
                    File.Move(_saveFile, GetBackupFileName(_saveFile));
                }
                catch (Exception e2)
                {
                    _log.LogError("Failed to move UI data file to backup: " + e2.ToString());
                }
            }

            return false;
        }

        internal void SaveData(WindowSerializer serializer, string source, RectTransform[] transforms)
        {
            try
            {
                Directory.CreateDirectory(_saveDirectory);

                if (File.Exists(_saveFile))
                {
                    string backupFile = GetBackupFileName(_saveFile);
                    File.Move(_saveFile, backupFile);
                    serializer.SaveData(_saveFile, source, transforms);

                    // Won't be reached in case of issues.
                    File.Delete(backupFile);
                }
                else
                {
                    serializer.SaveData(_saveFile, source, transforms);
                }
            }
            catch (Exception e)
            {
                _log.LogError(string.Format("Failed to save UI '{0}' data: {1}", _saveFile, e.ToString()));
            }
        }
    }
}

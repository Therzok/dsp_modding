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

        readonly List<WindowGroup> _registrations = new List<WindowGroup>(64);

        internal WindowManager(string saveDirectory)
        {
            _saveDirectory = saveDirectory;
        }

        internal WindowGroup RegisterWindows(string pluginName, RectTransform[] draggableWindows)
        {
            // TODO: We have no way to know when a new window is added or when a window is renamed.

            // Sort the windows for easier reading 
            Array.Sort(draggableWindows, (x, y) => StringComparer.Ordinal.Compare(x.name, y.name));
            _log.DevLog(draggableWindows, x => x.name, "UIWindows");

           return new WindowGroup(pluginName, draggableWindows, _registrations);
        }

        internal bool LoadData(WindowSerializer serializer, string source, Dictionary<string, RectTransform> transforms)
        {
            string saveFileName = Path.Combine(_saveDirectory, "ui.xml");
            if (File.Exists(saveFileName))
            {
                return serializer.LoadData(saveFileName, source, transforms);
            }

            return false;
        }

        internal void SaveData(WindowSerializer serializer, string source, RectTransform[] transforms)
        {
            Directory.CreateDirectory(_saveDirectory);

            string saveFileName = Path.Combine(_saveDirectory, "ui.xml");

            serializer.SaveData(saveFileName, source, transforms);
        }

        // UI Root/Overlay Canvas/In Game/Windows

        // Unhandled classes:
        // UIWindow where canDrag = true
        //
    }


}

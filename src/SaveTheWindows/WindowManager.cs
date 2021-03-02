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
    public sealed partial class WindowManager
    {
        readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource(nameof(WindowManager));
        readonly string _saveDirectory;
        readonly WindowSerializer _serializer;

        readonly List<WindowGroup> _registrations = new List<WindowGroup>(64);

        internal WindowManager(string saveDirectory, WindowSerializer serializer)
        {
            _saveDirectory = saveDirectory;
            _serializer = serializer;
        }

        internal IDisposable RegisterWindows(string pluginName, RectTransform[] draggableWindows)
        {
            // TODO: Public
            // TODO: We have no way to know when a new window is added or when a window is renamed.

            // Sort the windows for easier reading 
            Array.Sort(draggableWindows, (x, y) => StringComparer.Ordinal.Compare(x.name, y.name));
            _log.DevLog(draggableWindows, x => x.name != null ? x.name : "no name", "UIWindows");

            return new WindowGroup(pluginName, draggableWindows, _registrations);
        }

        internal void UnregisterWindow(string pluginName, string windowName)
        {
            // TODO: Public
        }

        internal void UnregisterWindows(string pluginName)
        {
            // TODO: Public
        }

        internal void LoadData(string gameName)
        {
            if (string.IsNullOrEmpty(gameName))
            {
                return;
            }

            string saveFileName = Path.Combine(_saveDirectory, gameName = ".xml");
            if (File.Exists(saveFileName))
            {
                _serializer.LoadData(saveFileName);
            }
        }

        internal void SaveData(string gameName)
        {
            Directory.CreateDirectory(_saveDirectory);

            string saveFileName = Path.Combine(_saveDirectory, gameName + ".xml");

            _serializer.SaveData(saveFileName, _registrations);
        }

        // UI Root/Overlay Canvas/In Game/Windows

        // Unhandled classes:
        // UIWindow where canDrag = true
        //
    }


}

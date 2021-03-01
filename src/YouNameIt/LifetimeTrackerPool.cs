using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BepInEx.Logging;

using UnityEngine;

namespace YouNameIt
{
    sealed partial class LifetimeTrackerPool
    {
        //ConfigEntry
        public static bool GalacticOnly = true;

        const int DefaultBufferSize = 8;

        readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource(nameof(LifetimeTrackerPool));
        Tracker[] _trackers = new Tracker[DefaultBufferSize];
        int _trackerCount;

        internal void Enable()
        {
            Hooks.Transport.Added += OnTransportAdded;
            Hooks.Transport.Removed += OnTransportRemoved;
            Hooks.StationWindow.Submit += OnTransportNameChanged;


            GameData data = GameMain.data;

            for (int i = 0; i < data.factoryCount; ++i)
            {
                PlanetFactory factory = data.factories[i];
                if (!factory.planet.factoryLoaded)
                {
                    continue;
                }

                // Manual list impl.
                if (_trackerCount == _trackers.Length)
                {
                    Array.Resize(ref _trackers, _trackers.Length * 2);
                }

                Tracker newTracker = _trackers[_trackerCount];
                if (newTracker == null)
                {
                    newTracker = new Tracker(_log);
                    _trackers[_trackerCount++] = newTracker;
                }

                // Start monitoring this planet.
                Debug.Assert(newTracker.Transport == null);
                newTracker.Track(factory.transport);
            }
        }

        internal void Disable()
        {
            Hooks.Transport.Added -= OnTransportAdded;
            Hooks.Transport.Removed -= OnTransportRemoved;
            Hooks.StationWindow.Submit -= OnTransportNameChanged;

            Clear();
        }

        internal void OnTransportNameChanged(UIStationWindow window, string name)
        {
            if (TryGetTracker(window.transport, out Tracker tracker))
            {
                tracker.NameChange(window.stationId);
            }
        }

        internal void OnTransportAdded(PlanetTransport transport, PrefabDesc desc, StationComponent station)
        {
            if (TryGetTracker(transport, out Tracker tracker))
            {
                tracker.Track(station);
            }
        }

        internal void OnTransportRemoved(PlanetTransport transport, int id)
        {
            if (TryGetTracker(transport, out Tracker tracker))
            {
                tracker.Untrack(id);
            }
        }

        bool TryGetTracker(PlanetTransport transport, out Tracker tracker)
        {
            for (int i = 0; i < _trackerCount; ++i)
            {
                tracker = _trackers[i];
                if (tracker.Transport == transport)
                {
                    return true;
                }
            }
            tracker = null;
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < _trackerCount; ++i)
            {
                _trackers[i].Clear();
            }

            _trackerCount = 0;
        }
    }
}

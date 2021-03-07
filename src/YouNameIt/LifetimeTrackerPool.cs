using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BepInEx.Configuration;
using BepInEx.Logging;

using TMPro;

using UnityEngine;

namespace YouNameIt
{
    sealed partial class LifetimeTrackerPool
    {
        const int DefaultBufferSize = 8;

        sealed class PoolFactory : PoolFactory<TransportTracker>
        {
            readonly ManualLogSource _log;
            readonly ConfigEntry<TrackedStations> _trackedStations;

            public PoolFactory(ManualLogSource log, ConfigEntry<TrackedStations> trackedStations)
            {
                _log = log;
                _trackedStations = trackedStations;
            }

            protected override TransportTracker OnCreate()
            {
                return new TransportTracker(_log, _trackedStations);
            }
        }

        readonly List<TransportTracker> _trackers = new List<TransportTracker>(DefaultBufferSize);
        readonly Pool<TransportTracker> _pool;

        public LifetimeTrackerPool(ConfigFile config)
        {
            var factory = new PoolFactory(
                BepInEx.Logging.Logger.CreateLogSource(nameof(LifetimeTrackerPool)),
                config.Bind(nameof(YouNameIt), nameof(TrackedStations), TrackedStations.Interplanetary)
            );

            _pool = new Pool<TransportTracker>(DefaultBufferSize, factory);
        }

        internal void Enable()
        {
            Hooks.Transport.Added += OnTransportAdded;
            Hooks.Transport.Removed += OnTransportRemoved;
            Hooks.StationWindow.Submit += OnTransportNameChanged;

            Hooks.Factory.Loaded += OnFactoryLoaded;
            Hooks.Factory.Unloading += OnFactoryUnloading;
        }

        void OnFactoryLoaded(PlanetData data)
        {
            TransportTracker tracker = _pool.Rent();
            _trackers.Add(tracker);

            tracker.Track(data.factory.transport);
        }

        void OnFactoryUnloading(PlanetData data)
        {
            if (data.factoryLoaded && TryGetTracker(data.factory.transport, out TransportTracker tracker))
            {
                tracker.Clear();
                _trackers.Remove(tracker);

                _pool.Return(tracker);
            }
        }

        internal void Disable()
        {
            Hooks.Factory.Loaded -= OnFactoryLoaded;
            Hooks.Factory.Unloading -= OnFactoryUnloading;

            Hooks.Transport.Added -= OnTransportAdded;
            Hooks.Transport.Removed -= OnTransportRemoved;
            Hooks.StationWindow.Submit -= OnTransportNameChanged;

            Clear();
        }

        internal void OnTransportNameChanged(UIStationWindow window, string name)
        {
            if (TryGetTracker(window.transport, out TransportTracker tracker))
            {
                tracker.NameChange(window.stationId);
            }
        }

        internal void OnTransportAdded(PlanetTransport transport, PrefabDesc desc, StationComponent station)
        {
            if (TryGetTracker(transport, out TransportTracker tracker))
            {
                tracker.Track(station);
            }
        }

        internal void OnTransportRemoved(PlanetTransport transport, int id)
        {
            if (TryGetTracker(transport, out TransportTracker tracker))
            {
                tracker.Untrack(id);
            }
        }

        bool TryGetTracker(PlanetTransport transport, out TransportTracker tracker)
        {
            for (int i = 0; i < _trackers.Count; ++i)
            {
                if ((tracker = _trackers[i]).Transport == transport)
                {
                    return true;
                }
            }
            tracker = null;
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < _trackers.Count; ++i)
            {
                _trackers[i].Clear();
                _pool.Return(_trackers[i]);
            }

            _trackers.Clear();
        }
    }
}

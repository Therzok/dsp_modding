﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BepInEx.Configuration;
using BepInEx.Logging;

using TMPro;

using UnityEngine;

namespace YouNameIt
{
    sealed class TransportTracker
    {
        readonly ManualLogSource _log;
        readonly ConfigEntry<TrackedStations> _trackedStations;

        TextMeshPro[] _texts = ArrayUtil.Empty<TextMeshPro>();

        public PlanetTransport Transport { get; private set; }
        GameObject _rootTextContainer;

        public TransportTracker(ManualLogSource log, ConfigEntry<TrackedStations> trackedStations)
        {
            _log = log;
            _trackedStations = trackedStations;
        }

        public void Track(PlanetTransport transport)
        {
            // Start monitoring this planet.
            Debug.Assert(transport != null, "Trying to subscribe null transport");
            Debug.Assert(Transport == null, $"Transport already registered {Transport.planet.id}");

            Transport = transport;

            _rootTextContainer = new GameObject(nameof(TransportTracker));
            _rootTextContainer.transform.SetParent(transport.planet.gameObject.transform, false);

            _texts = new TextMeshPro[transport.stationPool.Length];

            for (int i = 1; i < transport.stationCursor; ++i)
            {
                StationComponent station = transport.stationPool[i];
                if (station != null && station.id == i)
                {
                    Track(station);
                }
            }
        }

        public void Clear()
        {
            Debug.Assert(Transport != null, $"Trying to unsubscribe, but Transport is already null");

            Transport = null;

            for (int i = 1; i < _texts.Length; ++i)
            {
                TextMeshPro text = _texts[i];
                if (text != null)
                {
                    GameObject.Destroy(text.gameObject);
                    text.gameObject.SetActive(false);
                }
            }
        }

        static void ResizeArray<T>(ref T[] array, int minimum)
        {
            int length = array.Length;
            while (minimum > length)
            {
                length *= 2;
            }
            Array.Resize(ref array, length);
        }

        public void Track(StationComponent station)
        {
            if (!station.IsTracked(_trackedStations.Value))
            {
                return;
            }

            // Get point above station
            Vector3 newPos = station.shipDockPos + (station.shipDockRot * new Vector3(0, 10f, 0));

            ResizeArray(ref _texts, station.id);

            GameObject go;

            if (!TryGet(station.id, out TextMeshPro text))
            {
                go = GameObject.Instantiate(PrefabBlocks.PrefabManager.Instance.WorldText, _rootTextContainer.transform);
                text = go.GetComponent<TextMeshPro>();

                text.fontSizeMin = 60;

                _texts[station.id] = text;
            }
            else
            {
                go = text.gameObject;
            }

            // gameobject init
            go.name = "StationComponent_" + station.id.ToString();

            // transform init
            go.transform.localPosition = newPos;
            go.transform.LookAt(_rootTextContainer.transform.position);

            // text props

            string name = station.name != null ? station.name : "";

            text.text = name;

            go.SetActive(!string.IsNullOrEmpty(name));

            _log.DevLog("Station[" + station.id + "] added: " + name);
        }

        bool TryGet(int id, out TextMeshPro text)
        {
            text = (uint)id < (uint)_texts.Length ? _texts[id] : null;
            return text != null;
        }

        public void Untrack(int id)
        {
            if (TryGet(id, out TextMeshPro text))
            {
                text.gameObject.SetActive(false);

                _log.DevLog("Station " + id + " removed");
            }
        }

        public void NameChange(int id)
        {
            if (!TryGet(id, out TextMeshPro text))
            {
                return;
            }

            StationComponent original = Transport.stationPool[id];
            if (original.id == id && text.text != original.name)
            {
                text.text = original.name;

                // If they're different states, we need to toggle on/off.
                bool shouldBeActive = !string.IsNullOrEmpty(original.name);
                if (text.gameObject.activeSelf != shouldBeActive)
                {
                    text.gameObject.SetActive(shouldBeActive);
                }
            }
        }
    }
}

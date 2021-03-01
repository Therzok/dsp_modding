using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BepInEx.Logging;

using TMPro;

using UnityEngine;

namespace YouNameIt
{
    sealed partial class LifetimeTrackerPool
    {
        sealed class Tracker
        {
            readonly ManualLogSource _log;

            TextMeshPro[] _texts = ArrayUtil.Empty<TextMeshPro>();

            public PlanetTransport Transport { get; private set; }

            public Tracker(ManualLogSource log)
            {
                _log = log;
            }

            public void Track(PlanetTransport transport)
            {
                Transport = transport;

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
                Transport = null;

                for (int i = 0; i < _texts.Length; ++i)
                {
                    _texts[i].gameObject.SetActive(false);
                }
            }

            public void Track(StationComponent station)
            {
                if (GalacticOnly && !station.isStellar)
                {
                    return;
                }

                // Get point above station
                Vector3 newPos = station.shipDockPos + (station.shipDockRot * Vector3.up * 10);


                while (station.id > _texts.Length)
                {
                    Array.Resize(ref _texts, _texts.Length * 2);
                }

                Transform parentPlanet = Transport.planet.gameObject.transform;
                TextMeshPro text = _texts[station.id];
                GameObject go;

                if (text == null)
                {
                    go = GameObject.Instantiate(PrefabBlocks.PrefabManager.Instance.WorldText, parentPlanet);
                    text = go.GetComponent<TextMeshPro>();

                    text.fontSizeMin = 60;

                    _texts[station.id] = text;
                }
                else
                {
                    go = text.gameObject;
                }

                // gameobject init
                go.name = station.id.ToString() + " " + station.name;

                // transform init
                go.transform.localPosition = newPos;
                go.transform.LookAt(parentPlanet.position);

                // text props
                text.text = station.name;

                if (!string.IsNullOrEmpty(station.name))
                {
                    go.SetActive(true);

                    _log.DevLogInfo("Station[" + station.id + "] added: " + station.name);
                }
                else
                {
                    _log.DevLogInfo("Station[" + station.id + "] added inactive");
                }
            }

            bool TryGet(int id, out TextMeshPro text)
            {
                text = id < _texts.Length ? _texts[id] : null;
                return text != null;
            }

            public void Untrack(int id)
            {
                if (TryGet(id, out TextMeshPro text))
                {
                    text.gameObject.SetActive(false);

                    _log.DevLogInfo("Station " + id + " removed");
                }
            }

            public void NameChange(int id)
            {
                if (!TryGet(id, out TextMeshPro text))
                {
                    return;
                }

                StationComponent original = Transport.stationPool[id];
                if (original.id != id)
                {
                    return;
                }

                string oldName = text.text;
                string newName = original.name;
                if (oldName == newName)
                {
                    return;
                }

                text.text = original.name;

                // If they're different states, we need to toggle on/off.
                bool newEmpty = string.IsNullOrEmpty(newName);
                if (string.IsNullOrEmpty(oldName) ^ newEmpty)
                {
                    text.gameObject.SetActive(newEmpty);
                }
            }
        }
    }
}

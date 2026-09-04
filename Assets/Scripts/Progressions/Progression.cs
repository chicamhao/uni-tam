using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Progressions
{
    /// <summary>
    /// Chapter progression system — manages actor positioning across chapters.
    /// Plain C# service. Created by GameDriver, dependencies injected via Init().
    /// Discovers IPositionable actors via explicit registration.
    /// </summary>
    public sealed class Progression : IProgression
    {
        private readonly List<IPositionable> _positionables = new();
        private Transform[] _spawnPoints = Array.Empty<Transform>();
        private ChapterSettings _chapters;

        public void Init(ProgressionSettings progressSettings)
        {
            _spawnPoints = progressSettings.SpawnPoints;
            _chapters = progressSettings.ChapterSettings;
        }

        public void RegisterPositionable(IPositionable obj)
        {
            if (!_positionables.Contains(obj))
                _positionables.Add(obj);
        }

        /// <summary>
        /// Registers all IPositionable MonoBehaviours currently in the scene.
        /// Called by GameDriver after scene load.
        /// </summary>
        public void DiscoverPositionables()
        {
            var found = UnityEngine.Object.FindObjectsByType<MonoBehaviour>();
            foreach (var mb in found)
            {
                if (mb is IPositionable pos && !_positionables.Contains(pos))
                {
                    _positionables.Add(pos);
                }
            }
        }

        public void ApplyChapter(int chapter)
        {
            foreach (var pos in _positionables)
            {
                foreach (var entry in _chapters.Entries)
                {
                    if (entry.ActorID == pos.GetActorID() && entry.Chapter == chapter)
                    {
                        Transform spawnPoint = FindSpawnPoint(entry.SpawnPointID);
                        pos.ApplyState(entry, spawnPoint);
                        break;
                    }
                }
            }
        }

        private Transform FindSpawnPoint(string spawnPointID)
        {
            if (string.IsNullOrEmpty(spawnPointID)) return null;
            return _spawnPoints.FirstOrDefault(sp => sp != null && sp.name == spawnPointID);
        }
    }
}
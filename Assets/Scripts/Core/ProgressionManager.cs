using System;
using System.Collections.Generic;
using UnityEngine;
using Settings;
using Utility;

/// <summary>
/// Manages chapter transitions and IPositionable object states.
/// Plain C# singleton — registered by Bootstrapper, scene references passed via Init().
/// </summary>
public class ProgressionManager
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static ProgressionManager Instance => DIContainer.Get<ProgressionManager>();

    // ── State ──────────────────────────────────────────────────────────────────
    private List<IPositionable> _positionables = new();
    private Transform[] _spawnPoints = Array.Empty<Transform>();

    private ChapterSettings _chapters;
    private Dictionary<string, ChapterEntry> _chapterDb;

    /// <summary>
    /// Call once after the scene has loaded to wire up serialized references.
    /// Called by GameDriver.Awake().
    /// </summary>
    public void Init(Transform[] spawnPoints, ChapterSettings chapters)
    {
        _spawnPoints = spawnPoints ?? Array.Empty<Transform>();
        _chapters = chapters;
        _chapterDb = null; // force rebuild on next access

        // Discover all IPositionable objects already in the scene.
        var found = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsSortMode.None);
        foreach (var mb in found)
        {
            if (mb is IPositionable pos)
                _positionables.Add(pos);
        }
    }

    public void RegisterPositionable(IPositionable obj)
    {
        if (!_positionables.Contains(obj))
            _positionables.Add(obj);
    }

    /// <summary>
    /// Applies the current chapter state to all registered IPositionables.
    /// </summary>
    public void ApplyChapter(int chapter)
    {
        foreach (var pos in _positionables)
        {
            string key = $"{pos.GetActorID()}_{chapter}";
            if (ChapterDb.TryGetValue(key, out ChapterEntry entry))
            {
                Transform spawnPoint = FindSpawnPoint(entry.SpawnPointID);
                pos.ApplyState(entry, spawnPoint);
            }
        }
    }

    private Transform FindSpawnPoint(string spawnPointID)
    {
        if (string.IsNullOrEmpty(spawnPointID)) return null;
        foreach (var sp in _spawnPoints)
        {
            if (sp != null && sp.name == spawnPointID)
                return sp;
        }
        return null;
    }

    // ── Chapter Database ────────────────────────────────────────────────────────
    private Dictionary<string, ChapterEntry> ChapterDb
    {
        get
        {
            if (_chapterDb == null && _chapters != null)
            {
                _chapterDb = new Dictionary<string, ChapterEntry>();
                foreach (var entry in _chapters.Entries)
                {
                    string key = $"{entry.ActorID}_{entry.Chapter}";
                    _chapterDb[key] = entry;
                }
            }
            return _chapterDb;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only rebuild (call from a custom Editor or via the inspector on the
    /// GameDriver if needed). No [ContextMenu] since this is no longer a MonoBehaviour.
    /// </summary>
    public void RebuildDatabase()
    {
        _chapterDb = null;
        var _ = ChapterDb;
        Debug.Log($"Rebuilt chapter database with {(_chapterDb?.Count ?? 0)} entries.");
    }
#endif
}
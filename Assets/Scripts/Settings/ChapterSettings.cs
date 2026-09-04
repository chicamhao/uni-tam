using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    /// <summary>ScriptableObject containing a list of chapter entries for scene transitions.</summary>
    [CreateAssetMenu(fileName = "ChapterSettings", menuName = "ScriptableObjects/ChapterSettings", order = 1)]
    public sealed class ChapterSettings : ScriptableObject
    {
        public List<ChapterEntry> Entries = new();
    }

    /// <summary>Defines an actor's state at a given chapter — spawn point, animation, and visibility.</summary>
    [System.Serializable]
    public struct ChapterEntry
    {
        public string ActorID;
        public int Chapter;
        public string SpawnPointID;
        public AnimationClip Anim;
        public bool IsVisible;
    }
}
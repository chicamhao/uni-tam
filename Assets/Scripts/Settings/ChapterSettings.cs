using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    [CreateAssetMenu(fileName = "ChapterSettings", menuName = "ScriptableObjects/ChapterSettings", order = 1)]
    public sealed class ChapterSettings : ScriptableObject
    {
        public List<ChapterEntry> Entries = new();
    }

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
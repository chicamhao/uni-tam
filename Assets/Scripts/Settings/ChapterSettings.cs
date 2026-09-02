using System.Collections.Generic;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "ChapterSettings", menuName = "ScriptableObjects/ChapterSettings", order = 1)]
    public sealed class ChapterSettings : ScriptableObject
    {
        public List<ChapterEntry> Entries = new();
    }
}
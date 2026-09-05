using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    /// <summary>ScriptableObject configuring chapter progression — chapter data and default NPC cards.
    /// Spawn points are identified at runtime by GameObject tag "SpawnPoint".</summary>
    [CreateAssetMenu(fileName = "ProgressionSettings", menuName = "ScriptableObjects/ProgressionSettings", order = 1)]
    public sealed class ProgressionSettings : ScriptableObject
    {
        [Header("Chapter Configuration")]
        public ChapterSettings ChapterSettings;

        [Header("Default NPC Cards")]
        public List<CardDefinition> DefaultCards = new();

        [Header("Return Card")]
        public CardDefinition ReturnCard;
    }
}
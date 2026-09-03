using Settings;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    [CreateAssetMenu(fileName = "ProgressionSettings", menuName = "ScriptableObjects/ProgressionSettings", order = 1)]
    public sealed class ProgressionSettings : ScriptableObject
    {
        [Header("Spawn Points")]
        public Transform[] SpawnPoints;

        [Header("Chapter Configuration")]
        public ChapterSettings ChapterSettings;

        [Header("Default NPC Cards")]
        public List<CardDefinition> DefaultCards = new();

        [Header("Return Card")]
        public CardDefinition ReturnCard;
    }
}
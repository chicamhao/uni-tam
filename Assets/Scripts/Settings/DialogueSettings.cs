using System.Collections.Generic;
using Settings;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    [CreateAssetMenu(fileName = "DialogSettings", menuName = "ScriptableObjects/DialogSettings", order = 1)]
    public sealed class DialogueSettings : ScriptableObject
    {
        public List<DialogueEntry> Entries = new();
    }

    public sealed class DialogueEntry
    {
        [Header("Lookup")]
        [Tooltip("CardID that triggers this dialogue (matching CardSettings.CardID)")]
        public string CardID;

        [Tooltip("NPCID that this dialogue belongs to (matching NPC.NPCID)")]
        public string NPCID;

        [Header("Content")]
        public string NPCDisplayName;

        public List<DialogueLine> Lines;
    }

    [System.Serializable]
    public struct DialogueLine
    {
        public string Line;
        public float DisplayDuration;
        public ExpressionDefinition Expression;
    }

    [System.Serializable]
    public struct MorphTargetValue
    {
        public string name;
        public float value;
        public float blendInTime;
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    /// <summary>ScriptableObject holding all dialogue entries.</summary>
    [CreateAssetMenu(fileName = "DialogSettings", menuName = "ScriptableObjects/DialogSettings", order = 1)]
    public sealed class DialogueSettings : ScriptableObject
    {
        public List<DialogueEntry> Entries = new();
    }

    /// <summary>Represents a dialogue triggered by a card for a specific NPC, containing a list of lines.</summary>
    [System.Serializable]
    public sealed class DialogueEntry
    {
        [Header("Lookup")]
        [Tooltip("CardID that triggers this dialogue (matching CardSettings.CardID)")]
        public string CardID;

        [Tooltip("ActorID that this dialogue belongs to (matching Actor.ActorID)")]
        public string ActorID;

        [Header("Content")]
        public string ActorDisplayName;

        public List<DialogueLine> Lines;
    }

    /// <summary>A single line of dialogue with display duration and optional facial expression.</summary>
    [System.Serializable]
    public struct DialogueLine
    {
        public string Line;
        public float DisplayDuration;
        public ExpressionDefinition Expression;
    }

    /// <summary>Defines a morph target weight and blend time for facial expressions.</summary>
    [System.Serializable]
    public struct MorphTargetValue
    {
        public string name;
        public float value;
        public float blendInTime;
    }
}
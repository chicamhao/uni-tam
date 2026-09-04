using System;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    /// <summary>Value object pairing the unique key and display name for an actor.</summary>
    [Serializable]
    public sealed class ActorIdentifier
    {
        [SerializeField] private string _actorID;
        [SerializeField] private string _displayName;

        /// <summary>Unique identifier matching DialogueSettings entries.</summary>
        public string ActorID => _actorID;
        /// <summary>Display name shown in UI prompts and toasts.</summary>
        public string DisplayName => _displayName;
    }
}
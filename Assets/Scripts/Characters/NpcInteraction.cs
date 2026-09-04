using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Interfaces;

namespace Assets.Scripts.Characters
{
    /// <summary>Handles player interaction with NPCs (dialogue trigger, highlighting). Attached to NPC model.</summary>
    [DisallowMultipleComponent]
    public sealed class Interaction : MonoBehaviour, IInteractable
    {
        /// <summary>Set by GameDriver on Awake().</summary>
        public IDialogue DialogueRef { get; set; }

        private Actor _actor;
        private MaterialHandle _materialHandle;

        private void Awake()
        {
            _actor = GetComponentInParent<Actor>();
            _materialHandle = GetComponent<MaterialHandle>();
            Assert.IsNotNull(_actor, "Interaction requires an Actor on the parent or same GameObject.");
        }

        public void Interact() => DialogueRef?.OpenCardSelectionForActor(_actor);

        public string GetPrompt() => $"Talk to {_actor.Identifier.DisplayName}";

        public void SetHighlight(bool highlighted) => _materialHandle?.SetHighlight(highlighted);
    }
}
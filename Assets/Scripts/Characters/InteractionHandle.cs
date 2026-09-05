using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Interfaces;

namespace Assets.Scripts.Characters
{
    /// <summary>Handles player interaction with NPCs (dialogue trigger, highlighting). Attached to NPC model.</summary>
    [DisallowMultipleComponent]
    public sealed class InteractionHandle : MonoBehaviour, IInteractable
    {
        /// <summary>Set by GameDriver on Awake().</summary>
        public IDialogue DialogueRef { get; set; }

        private Actor _actor;
        private MaterialHandle _materialHandle;

        private void Awake()
        {
            _actor = GetComponentInParent<Actor>();
            _materialHandle = GetComponent<MaterialHandle>();
            Assert.IsNotNull(_actor);
        }

        public void Interact() => DialogueRef?.OpenCardSelectionForActor(_actor);

        public string GetPrompt() => $"Talk to {_actor.Identifier.DisplayName}";

        public void SetHighlight(bool highlighted) => _materialHandle?.SetHighlight(highlighted);
    }
}

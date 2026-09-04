using System;
using System.Collections.Generic;
using Assets.Scripts.Characters;
using Assets.Scripts.Settings;
using Assets.Scripts.Context;

namespace Assets.Scripts.Interfaces
{
    /// <summary>Defines the dialogue system contract — starting, ending, and skipping lines, and card selection.</summary>
    public interface IDialogue
    {
        DialogueContext Context { get; }
        bool SkipCurrentLine { get; set; }
        event Action<DialogueEntry, Actor> OnDialogueStarted;
        event Action OnDialogueEnded;
        event Action<DialogueLine> OnLineChanged;
        event Action<Actor> OnCardSelectionRequested;
        void Init(DialogueSettings settings);
        bool TryGetDialogue(string cardID, string actorID, out DialogueEntry entry);
        void StartDialogue(DialogueEntry entry, Actor npc);
        void EndDialogue();
        void Update();
        void OpenCardSelectionForActor(Actor npc);
        void OnCardSelected(CardDefinition selectedCard, Actor npc);
        void RequestSkip();
    }
}
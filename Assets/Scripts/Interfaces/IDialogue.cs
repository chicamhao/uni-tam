using System;
using System.Collections.Generic;
using Assets.Scripts.Characters;
using Assets.Scripts.Settings;
using Settings;

namespace Assets.Scripts.Interfaces
{
    public interface IDialogue
    {
        bool IsDialogueActive { get; }
        NPC CurrentNPC { get; }
        DialogueEntry CurrentEntry { get; }
        int CurrentLineIndex { get; }
        bool SkipCurrentLine { get; set; }
        event Action<DialogueEntry, NPC> OnDialogueStarted;
        event Action OnDialogueEnded;
        event Action<DialogueLine> OnLineChanged;
        event Action<NPC> OnCardSelectionRequested;
        void Init(DialogueSettings settings);
        bool TryGetDialogue(string cardID, string npcID, out DialogueEntry entry);
        void StartDialogue(DialogueEntry entry, NPC npc);
        void EndDialogue();
        void Update();
        void OpenCardSelectionForNPC(NPC npc);
        void OnCardSelected(CardDefinition selectedCard, NPC npc);
        void RequestSkip();
    }
}
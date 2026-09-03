using Assets.Scripts.Characters;
using Assets.Scripts.Settings;
using Settings;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IGameplayScene
    {
        int CurrentChapter { get; set; }
        void Init(Camera playerCamera, IDialogue dialogue, IProgression progression);
        void Tick();
        void HandleDialogueStarted(DialogueEntry entry, NPC npc);
        void HandleDialogueEnded();
        void AdvanceChapter();
    }
}
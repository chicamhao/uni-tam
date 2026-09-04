using UnityEngine;
using Assets.Scripts.Characters;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Interfaces
{
    /// <summary>Directs scene-level presentation modes — camera swap, cursor/input state, and chapter progression.</summary>
    public interface IDirector
    {
        int CurrentChapter { get; set; }
        void Init(Camera playerCamera, IDialogue dialogue, IProgression progression);
        void Tick();
        void HandleDialogueStarted(DialogueEntry entry, Actor npc);
        void HandleDialogueEnded();
        void AdvanceChapter();
    }
}
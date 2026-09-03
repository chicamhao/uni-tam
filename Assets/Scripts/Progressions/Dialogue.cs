using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Characters;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Settings;
using Settings;
using UnityEngine.Assertions;
using Utility;

namespace Assets.Scripts.Progressions
{
    /// <summary>
    /// Dialogue system managing card-triggered conversations with NPCs.
    /// Plain C# service — created by GameDriver, dependencies injected via Init().
    /// No static access. Communicates via C# events consumed by Gui and GameplayScene.
    /// </summary>
    public sealed class Dialogue : IDialogue
    {
        // ─── State ────────────────────────────────────────────────────────────
        public bool IsDialogueActive { get; private set; }
        public NPC CurrentNPC { get; private set; }
        public DialogueEntry CurrentEntry { get; private set; }
        public int CurrentLineIndex { get; private set; }
        public bool SkipCurrentLine { get; set; }

        // ─── Events (consumed by Gui / GameplayScene for UI / camera) ─────────
        public event Action<DialogueEntry, NPC> OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<DialogueLine> OnLineChanged;
        public event Action<NPC> OnCardSelectionRequested;

        // ─── Storage: key = "{CardID}_{NPCID}" ───────────────────────────────
        private readonly Dictionary<string, DialogueEntry> _dialogueDatabase = new();

        // ─── Line timer (non-MonoBehaviour, driven by GameplayScene.Update) ────
        private readonly Timer _lineTimer = new();

        // ─── Injected dependencies ────────────────────────────────────────────
        private readonly IPlayerState _playerState;

        public Dialogue(IPlayerState playerState)
        {
            _playerState = playerState;
        }

        public void Init(DialogueSettings settings)
        {
            foreach (var e in settings.Entries)
            {
                RegisterDialogue(e);
            }
        }

        // ─── Database ─────────────────────────────────────────────────────────
        public void RegisterDialogue(DialogueEntry entry)
        {
            _dialogueDatabase[$"{entry.CardID}_{entry.NPCID}"] = entry;
        }

        public bool TryGetDialogue(string cardID, string npcID, out DialogueEntry entry)
        {
            return _dialogueDatabase.TryGetValue($"{cardID}_{npcID}", out entry);
        }

        // ─── Lifecycle ────────────────────────────────────────────────────────
        public void StartDialogue(DialogueEntry entry, NPC npc)
        {
            if (IsDialogueActive) return;

            CurrentEntry = entry;
            CurrentNPC = npc;
            CurrentLineIndex = 0;
            IsDialogueActive = true;
            SkipCurrentLine = false;
            _lineTimer.Dispose();

            var firstLine = entry.Lines[0];
            ApplyLine(firstLine);

            OnDialogueStarted?.Invoke(entry, npc);
        }

        public void EndDialogue()
        {
            if (!IsDialogueActive) return;

            IsDialogueActive = false;
            _lineTimer.Dispose();

            CurrentNPC = null;
            CurrentEntry = null;
            CurrentLineIndex = 0;
            SkipCurrentLine = false;

            OnDialogueEnded?.Invoke();
        }

        public void Update()
        {
            if (!IsDialogueActive) return;

            var lines = CurrentEntry.Lines;

            if (SkipCurrentLine || _lineTimer.Update())
            {
                SkipCurrentLine = false;
                CurrentLineIndex++;

                if (CurrentLineIndex >= lines.Count)
                {
                    EndDialogue();
                    return;
                }

                ApplyLine(lines[CurrentLineIndex]);
            }
        }

        private void ApplyLine(DialogueLine line)
        {
            Assert.IsNotNull(CurrentNPC, "CurrentNPC is null when applying dialogue line.");
            CurrentNPC.ApplyExpression(line.Expression);

            _lineTimer.SetDuration(line.DisplayDuration);
            OnLineChanged?.Invoke(line);
        }

        // ─── Card Selection ───────────────────────────────────────────────────
        public void OpenCardSelectionForNPC(NPC npc)
        {
            var owned = _playerState.OwnedCards;
            var usable = owned.Where(c => c.TargetNPCIDs.Count == 0 || c.TargetNPCIDs.Contains(npc.NPCID)).ToList();

            OnCardSelectionRequested?.Invoke(npc);
        }

        public void OnCardSelected(CardDefinition selectedCard, NPC npc)
        {
            Assert.IsTrue(TryGetDialogue(selectedCard.CardID, npc.NPCID, out var entry));
            StartDialogue(entry, npc);
        }

        // ─── Line skip ────────────────────────────────────────────────────────
        public void RequestSkip()
        {
            if (IsDialogueActive)
            {
                SkipCurrentLine = true;
            }
        }
    }
}
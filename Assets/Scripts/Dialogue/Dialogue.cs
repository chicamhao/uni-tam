using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

public sealed class Dialogue
    {
        public static Dialogue Instance => DIContainer.Get<Dialogue>();

        // ─── State ────────────────────────────────────────────────────────────
        public bool IsDialogueActive { get; private set; }
        public NPC CurrentNPC { get; private set; }
        public DialogueEntry CurrentEntry { get; private set; }
        public int CurrentLineIndex { get; private set; }
        public bool SkipCurrentLine { get; set; }

        // ─── Events (consumed by UIManager / GameplayScene for UI / camera) ───
        public event Action<DialogueEntry, NPC> OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<DialogueLine> OnLineChanged;
        public event Action<NPC> OnCardSelectionRequested;

        // ─── Storage: key = "{CardID}_{NPCID}" ───────────────────────────────
        private readonly Dictionary<string, DialogueEntry> _dialogueDatabase = new();

        // ─── Line timer (non-MonoBehaviour, driven by GameplayScene.Update) ────
        private readonly Timer _lineTimer = new();

        public Dialogue()
        {
            // Auto-register all DialogueEntry ScriptableObjects from Resources/Dialogues
            var entries = Resources.LoadAll<DialogueEntry>("Dialogues");
            foreach (var entry in entries)
            {
                RegisterDialogue(entry.CardID, entry.NPCID, entry);
            }
        }

        // ─── Database ─────────────────────────────────────────────────────────
        public void RegisterDialogue(string cardID, string npcID, DialogueEntry entry)
        {
            _dialogueDatabase[$"{cardID}_{npcID}"] = entry;
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

            // Show the first line immediately
            var firstLine = entry.Lines[0];
            ApplyLine(firstLine);

            // Fire event so subscribers swap camera, show UI, etc.
            OnDialogueStarted?.Invoke(entry, npc);
        }

        public void EndDialogue()
        {
            if (!IsDialogueActive) return;

            IsDialogueActive = false;
            _lineTimer.Dispose();

            CurrentNPC = null;
            CurrentEntry = default;
            CurrentLineIndex = 0;
            SkipCurrentLine = false;

            OnDialogueEnded?.Invoke();
        }

        /// <summary>
        /// Called every frame by GameplayScene. Drives the line timer.
        /// </summary>
        public void Update()
        {
            if (!IsDialogueActive) return;

            var lines = CurrentEntry.Lines;

            // If skip was requested, or the timer expired, advance.
            if (SkipCurrentLine || _lineTimer.Update())
            {
                SkipCurrentLine = false;
                CurrentLineIndex++;

                if (CurrentLineIndex >= lines.Count)
                {
                    EndDialogue();
                    return;
                }

                var nextLine = lines[CurrentLineIndex];
                ApplyLine(nextLine);
            }
        }

        private void ApplyLine(DialogueLine line)
        {
            // Apply facial expression
            if (CurrentNPC != null)
                CurrentNPC.ApplyExpression(line.Expression);

            // Set timer for this line's display duration
            _lineTimer.SetDuration(line.DisplayDuration);

            // Notify UI
            OnLineChanged?.Invoke(line);
        }

        // ─── Card Selection ───────────────────────────────────────────────────
        public void OpenCardSelectionForNPC(NPC npc)
        {
            var owned = PlayerState.Instance.OwnedCards;
            var usable = owned.Where(c => c.TargetNPCIDs.Count == 0 || c.TargetNPCIDs.Contains(npc.NPCID)).ToList();

            if (usable.Count == 0)
            {
                UIManager.Instance?.ShowToast("No usable cards.");
                return;
            }

            // Store the NPC reference so the UI callback can find it
            OnCardSelectionRequested?.Invoke(npc);
        }

        /// <summary>
        /// Called by the card selection UI when a card is chosen.
        /// </summary>
        public void OnCardSelected(CardData selectedCard, NPC npc)
        {
            if (TryGetDialogue(selectedCard.CardID, npc.NPCID, out var entry))
            {
                StartDialogue(entry, npc);
            }
            else
            {
                UIManager.Instance?.ShowToast("No dialogue for this card.");
            }
        }

        // ─── Line skip (for SkipLineAction) ──────────────────────────────────
        public void RequestSkip()
        {
            if (IsDialogueActive)
            {
                SkipCurrentLine = true;
            }
        }
    }
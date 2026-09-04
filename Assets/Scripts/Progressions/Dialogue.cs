using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using Assets.Scripts.Characters;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Settings;
using Utility;
using Assets.Scripts.Context;

namespace Assets.Scripts.Progressions
{
    /// <summary>
    /// Dialogue system managing card-triggered conversations with NPCs.
    /// </summary>
    public sealed class Dialogue : IDialogue
    {
        private readonly DialogueContext _context = new();
        public DialogueContext Context => _context;

        public bool SkipCurrentLine { get; set; }

        public event Action<DialogueEntry, Actor> OnDialogueStarted;

        public event Action OnDialogueEnded;
        public event Action<DialogueLine> OnLineChanged;
        public event Action<Actor> OnCardSelectionRequested;

        private readonly Dictionary<string, DialogueEntry> _dialogueDatabase = new();

        private readonly Timer _lineTimer = new();

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

        public void RegisterDialogue(DialogueEntry entry)
        {
            _dialogueDatabase[$"{entry.CardID}_{entry.ActorID}"] = entry;
        }

        public bool TryGetDialogue(string cardID, string actorID, out DialogueEntry entry)
        {
            return _dialogueDatabase.TryGetValue($"{cardID}_{actorID}", out entry);
        }

        public void StartDialogue(DialogueEntry entry, Actor npc)
        {
            if (_context.IsDialogueActive) return;

            _context.CurrentEntry = entry;
            _context.CurrentActor = npc;
            _context.CurrentLineIndex = 0;
            _context.IsDialogueActive = true;
            SkipCurrentLine = false;
            _lineTimer.Dispose();

            var firstLine = entry.Lines[0];
            ApplyLine(firstLine);

            OnDialogueStarted?.Invoke(entry, npc);
        }

        public void EndDialogue()
        {
            if (!_context.IsDialogueActive) return;

            _context.IsDialogueActive = false;
            _lineTimer.Dispose();

            _context.CurrentActor = null;
            _context.CurrentEntry = null;
            _context.CurrentLineIndex = 0;
            SkipCurrentLine = false;

            OnDialogueEnded?.Invoke();
        }

        public void Update()
        {
            if (!_context.IsDialogueActive) return;

            var lines = _context.CurrentEntry.Lines;

            if (SkipCurrentLine || _lineTimer.Update())
            {
                SkipCurrentLine = false;
                _context.CurrentLineIndex++;

                if (_context.CurrentLineIndex >= lines.Count)
                {
                    EndDialogue();
                    return;
                }

                ApplyLine(lines[_context.CurrentLineIndex]);
            }
        }

        private void ApplyLine(DialogueLine line)
        {
            Assert.IsNotNull(_context.CurrentActor, "CurrentActor is null when applying dialogue line.");
            var expr = _context.CurrentActor.GetComponent<Expression>();
            if (expr != null) expr.ApplyExpression(line.Expression);

            _lineTimer.SetDuration(line.DisplayDuration);
            OnLineChanged?.Invoke(line);
        }

        public void OpenCardSelectionForActor(Actor npc)
        {
            var owned = _playerState.OwnedCards;
            var usable = owned.Where(c => c.TargetActorIDs.Count == 0 || c.TargetActorIDs.Any(t => t.ActorID == npc.Identifier.ActorID)).ToList();

            OnCardSelectionRequested?.Invoke(npc);
        }

        public void OnCardSelected(CardDefinition selectedCard, Actor npc)
        {
            Assert.IsTrue(TryGetDialogue(selectedCard.CardID, npc.Identifier.ActorID, out var entry));
            StartDialogue(entry, npc);
        }

        public void RequestSkip()
        {
            if (_context.IsDialogueActive)
            {
                SkipCurrentLine = true;
            }
        }
    }
}

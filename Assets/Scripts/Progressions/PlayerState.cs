using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using Settings;

namespace Assets.Scripts.Progressions
{
    /// <summary>
    /// Player inventory — tracks owned cards.
    /// Plain C# service. Depends on IGui for toast notifications.
    /// Created by GameDriver, dependencies injected via Init().
    /// </summary>
    public sealed class PlayerState : IPlayerState
    {
        // ── State ─────────────────────────────────────────────────────────────
        private readonly List<CardDefinition> _ownedCards = new();
        public IReadOnlyList<CardDefinition> OwnedCards => _ownedCards.AsReadOnly();

        private CardDefinition _cardReturn;
        private readonly List<CardDefinition> _defaultNPCCards = new();
        private bool _initialized;

        // ── Injected dependencies ─────────────────────────────────────────────
        private IGui _gui;

        public PlayerState(IGui gui)
        {
            _gui = gui;
        }

        public void Init(CardDefinition cardReturn, List<CardDefinition> defaultNPCCards)
        {
            if (_initialized) return;
            _initialized = true;

            _cardReturn = cardReturn;
            _defaultNPCCards.Clear();
            if (defaultNPCCards != null)
                _defaultNPCCards.AddRange(defaultNPCCards);

            if (_cardReturn != null)
                GrantCard(_cardReturn);
            foreach (var card in _defaultNPCCards)
            {
                if (card != null)
                    GrantCard(card);
            }
        }

        public void GrantCard(CardDefinition card)
        {
            if (card == null) return;
            if (!_ownedCards.Contains(card))
            {
                _ownedCards.Add(card);
                _gui.ShowToast($"Obtained card: {card.DisplayName}");
            }
        }

        public bool HasCard(CardDefinition card) => _ownedCards.Contains(card);
    }
}
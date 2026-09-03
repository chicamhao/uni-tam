using System;
using System.Collections.Generic;
using Settings;
using Utility;

/// <summary>
/// Persistent singleton holding the player's owned cards.
/// Plain C# — registered by Bootstrapper, card references passed via Init().
/// </summary>
public class PlayerState
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static PlayerState Instance
    {
        get
        {
            if (DIContainer.TryGet<PlayerState>(out var instance))
                return instance;
            throw new InvalidOperationException(
                "PlayerState not registered. Bootstrapper should have called DIContainer.Inject().");
        }
    }

    // ── State ──────────────────────────────────────────────────────────────────
    private readonly List<CardData> _ownedCards = new();
    public IReadOnlyList<CardData> OwnedCards => _ownedCards.AsReadOnly();

    private CardData _cardReturn;
    private readonly List<CardData> _defaultNPCCards = new();
    private bool _initialized;

    /// <summary>
    /// Called by GameDriver.Awake() after scene load to supply inspector-assigned cards.
    /// </summary>
    public void Init(CardData cardReturn, List<CardData> defaultNPCCards)
    {
        if (_initialized) return;
        _initialized = true;

        _cardReturn = cardReturn;
        _defaultNPCCards.Clear();
        if (defaultNPCCards != null)
            _defaultNPCCards.AddRange(defaultNPCCards);

        // Grant default cards on game start
        if (_cardReturn != null)
            GrantCard(_cardReturn);
        foreach (var card in _defaultNPCCards)
        {
            if (card != null)
                GrantCard(card);
        }
    }

    public void GrantCard(CardData card)
    {
        if (card == null) return;
        if (!_ownedCards.Contains(card))
        {
            _ownedCards.Add(card);
            UIManager.Instance?.ShowToast($"Obtained card: {card.DisplayName}");
        }
    }

    public bool HasCard(CardData card)
    {
        return _ownedCards.Contains(card);
    }
}
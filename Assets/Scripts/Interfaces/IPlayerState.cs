using System.Collections.Generic;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Interfaces
{
    /// <summary>Defines the player state contract — card inventory management.</summary>
    public interface IPlayerState
    {
        IReadOnlyList<CardDefinition> OwnedCards { get; }
        void Init(CardDefinition cardReturn, List<CardDefinition> defaultActorCards);
        void GrantCard(CardDefinition card);
        bool HasCard(CardDefinition card);
    }
}
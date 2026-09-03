using System.Collections.Generic;
using Settings;

namespace Assets.Scripts.Interfaces
{
    public interface IPlayerState
    {
        IReadOnlyList<CardDefinition> OwnedCards { get; }
        void Init(CardDefinition cardReturn, List<CardDefinition> defaultNPCCards);
        void GrantCard(CardDefinition card);
        bool HasCard(CardDefinition card);
    }
}
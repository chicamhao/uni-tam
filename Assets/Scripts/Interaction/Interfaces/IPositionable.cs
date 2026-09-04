using Assets.Scripts.Settings;
using UnityEngine;

namespace Assets.Scripts.Interaction.Interfaces
{
    /// <summary>Defines an actor that can be positioned and animated per chapter state.</summary>
    public interface IPositionable
    {
        string GetActorID();
        void ApplyState(ChapterEntry state, Transform spawnPoint);
    }
}
using Assets.Scripts.Settings;
using UnityEngine;

namespace Assets.Scripts.Interaction.Interfaces
{
    public interface IPositionable
    {
        string GetActorID();
        void ApplyState(ChapterEntry state, Transform spawnPoint);
    }
}
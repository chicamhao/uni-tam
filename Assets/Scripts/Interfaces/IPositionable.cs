using Assets.Scripts.Settings;
using UnityEngine;

namespace Interfaces
{
    public interface IPositionable
    {
        string GetActorID();
        void ApplyState(ChapterEntry state, Transform spawnPoint);
    }
}
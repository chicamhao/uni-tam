using UnityEngine;

public interface IPositionable
{
    string GetActorID();
    void ApplyState(ChapterEntry state, Transform spawnPoint);
}
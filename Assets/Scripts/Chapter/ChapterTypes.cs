using UnityEngine;

[System.Serializable]
public struct ChapterEntry
{
    public string ActorID;          // e.g. "npc_guard" or "player"
    public int Chapter;
    public string SpawnPointID;
    public AnimationClip Anim;
    public bool bVisible;
}
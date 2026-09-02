using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Game/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Card Info")]
    public string CardID;
    public string DisplayName;
    [TextArea(3, 5)]
    public string Description;
    public Texture2D Icon;

    [Header("Behaviour")]
    public bool IsReturnCard;
    public List<string> TargetNPCIDs; // empty = usable on all NPCs
}
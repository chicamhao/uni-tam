using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject defining a dialogue entry tied to a card + NPC pair.
/// Create assets via Assets → Create → ScriptableObjects → DialogueEntry.
/// Place them under Assets/Resources/Dialogues/ so they auto-register at startup.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogueEntry", menuName = "ScriptableObjects/DialogueEntry", order = 1)]
public class DialogueEntry : ScriptableObject
{
    [Header("Lookup")]
    [Tooltip("CardID that triggers this dialogue (matching CardData.CardID)")]
    public string CardID;

    [Tooltip("NPCID that this dialogue belongs to (matching NPC.NPCID)")]
    public string NPCID;

    [Header("Content")]
    public string NPCDisplayName;

    public List<DialogueLine> Lines;
}
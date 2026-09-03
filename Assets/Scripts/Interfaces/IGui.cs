using System;
using System.Collections.Generic;
using Assets.Scripts.Characters;
using Assets.Scripts.Settings;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Interfaces
{
    public interface IGui
    {
        void Tick(float dt);
        void FadeToBlack(float duration = -1f);
        void FadeFromBlack(float duration = -1f);
        void ShowToast(string message);
        void ShowCardSelection(List<CardDefinition> availableCards, Action<CardDefinition> onCardSelected);
        void HideCardSelection();
        void HandleDialogueStarted(DialogueEntry entry, NPC npc);
        void HandleDialogueEnded();
        void HandleLineChanged(DialogueLine line);
        void Init(Image fadeOverlay, TextMeshProUGUI toastText, GameObject dialoguePanel,
                  TextMeshProUGUI npcNameText, TextMeshProUGUI lineText,
                  GameObject cardSelectionPanel, Transform cardListContainer,
                  GameObject cardButtonPrefab);
    }
}
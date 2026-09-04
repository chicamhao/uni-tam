using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Characters;
using Assets.Scripts.Settings;
using Assets.Scripts.Context;

namespace Assets.Scripts.Interfaces
{
    /// <summary>Defines the GUI system contract — fades, toasts, card selection, and dialogue display.</summary>
    public interface IGui
    {
        void Tick(float dt);
        void FadeToBlack(float duration = -1f);
        void FadeFromBlack(float duration = -1f);
        void ShowToast(string message);
        void ShowCardSelection(List<CardDefinition> availableCards, Action<CardDefinition> onCardSelected);
        void HideCardSelection();
        void HandleDialogueStarted(DialogueEntry entry, Actor npc);
        void HandleDialogueEnded();
        void HandleLineChanged(DialogueLine line);
        void Init(GuiContext config);
    }
}
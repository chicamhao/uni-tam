using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Characters;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Settings;
using Assets.Scripts.Context;

namespace Assets.Scripts.UI
{
    public sealed class Gui : IGui
    {
        public float FadeDefaultDuration { get; private set; } = 1.5f;
        public float ToastDuration { get; private set; } = 2.5f;

        private Image _fadeOverlay;
        private TextMeshProUGUI _toastText;
        private GameObject _dialoguePanel;
        private TextMeshProUGUI _npcNameText;
        private TextMeshProUGUI _lineText;
        private GameObject _cardSelectionPanel;
        private Transform _cardListContainer;
        private GameObject _cardButtonPrefab;

        private readonly List<GameObject> _cachedCardButtons = new();

        private enum FadeState { None, FadingToBlack, FadingFromBlack }
        private FadeState _fadeState;
        private float _fadeElapsed;
        private float _fadeDuration;

        private enum ToastState { None, Showing, FadingOut }
        private ToastState _toastState;
        private float _toastElapsed;

        public void Init(GuiContext config)
        {
            _fadeOverlay = config.FadeOverlay;
            _toastText = config.ToastText;
            _dialoguePanel = config.DialoguePanel;
            _npcNameText = config.NpcNameText;
            _lineText = config.LineText;
            _cardSelectionPanel = config.CardSelectionPanel;
            _cardListContainer = config.CardListContainer;
            _cardButtonPrefab = config.CardButtonPrefab;
        }

        public void Tick(float dt)
        {
            TickFade(dt);
            TickToast(dt);
        }

        private void TickFade(float dt)
        {
            if (_fadeState == FadeState.None || _fadeOverlay == null) return;

            _fadeElapsed += dt;
            float t = Mathf.Clamp01(_fadeElapsed / _fadeDuration);

            Color c = _fadeOverlay.color;

            if (_fadeState == FadeState.FadingToBlack)
            {
                c.a = Mathf.Lerp(0f, 1f, t);
                if (t >= 1f) _fadeState = FadeState.None;
            }
            else
            {
                c.a = Mathf.Lerp(1f, 0f, t);
                if (t >= 1f) _fadeState = FadeState.None;
            }

            _fadeOverlay.color = c;
        }

        private void TickToast(float dt)
        {
            if (_toastState == ToastState.None || _toastText == null) return;

            _toastElapsed += dt;

            if (_toastState == ToastState.Showing)
            {
                if (_toastElapsed >= ToastDuration)
                {
                    _toastState = ToastState.FadingOut;
                    _toastElapsed = 0f;
                }
            }
            else
            {
                float t = Mathf.Clamp01(_toastElapsed / 0.5f);
                Color c = _toastText.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                _toastText.color = c;

                if (t >= 1f)
                {
                    _toastText.gameObject.SetActive(false);
                    _toastState = ToastState.None;
                }
            }
        }

        // ── Dialogue UI ───────────────────────────────────────────────────────

        public void HandleDialogueStarted(DialogueEntry entry, Actor _)
        {
            if (_dialoguePanel != null) _dialoguePanel.SetActive(true);
            if (_npcNameText != null) _npcNameText.text = entry.ActorDisplayName;
        }

        public void HandleDialogueEnded()
        {
            if (_dialoguePanel != null) _dialoguePanel.SetActive(false);
        }

        public void HandleLineChanged(DialogueLine line)
        {
            if (_lineText != null) _lineText.text = line.Line;
        }

        // ── Fade ──────────────────────────────────────────────────────────────

        public void FadeToBlack(float duration = -1f)
        {
            if (_fadeOverlay == null) return;
            _fadeState = FadeState.FadingToBlack;
            _fadeElapsed = 0f;
            _fadeDuration = duration < 0f ? FadeDefaultDuration : duration;
        }

        public void FadeFromBlack(float duration = -1f)
        {
            if (_fadeOverlay == null) return;
            _fadeState = FadeState.FadingFromBlack;
            _fadeElapsed = 0f;
            _fadeDuration = duration < 0f ? FadeDefaultDuration : duration;
        }

        // ── Toast ─────────────────────────────────────────────────────────────

        public void ShowToast(string message)
        {
            if (_toastText == null) return;

            _toastText.text = message;
            _toastText.gameObject.SetActive(true);

            Color c = _toastText.color;
            c.a = 1f;
            _toastText.color = c;

            _toastState = ToastState.Showing;
            _toastElapsed = 0f;
        }

        // ── Card Selection UI ─────────────────────────────────────────────────

        public void ShowCardSelection(List<CardDefinition> availableCards, Action<CardDefinition> onCardSelected)
        {
            if (_cardSelectionPanel == null) return;
            _cardSelectionPanel.SetActive(true);

            int cardCount = availableCards.Count;

            // Deactivate all cached buttons, then reactivate/instantiate as needed
            for (int i = 0; i < _cachedCardButtons.Count; i++)
                _cachedCardButtons[i].SetActive(false);

            // Ensure we have enough buttons
            for (int i = _cachedCardButtons.Count; i < cardCount; i++)
            {
                GameObject btnObj = UnityEngine.Object.Instantiate(_cardButtonPrefab, _cardListContainer);
                _cachedCardButtons.Add(btnObj);
            }

            // Setup each button with its card data
            for (int i = 0; i < cardCount; i++)
            {
                GameObject btnObj = _cachedCardButtons[i];
                btnObj.SetActive(true);
                var btn = btnObj.GetComponent<Card>();
                int capturedIndex = i;
                if (btn != null)
                    btn.Setup(availableCards[i], () =>
                    {
                        _cardSelectionPanel.SetActive(false);
                        onCardSelected?.Invoke(availableCards[capturedIndex]);
                    });
            }

            // Deactivate any extra cached buttons
            for (int i = cardCount; i < _cachedCardButtons.Count; i++)
                _cachedCardButtons[i].SetActive(false);
        }

        public void HideCardSelection()
        {
            if (_cardSelectionPanel != null)
                _cardSelectionPanel.SetActive(false);
        }
    }
}

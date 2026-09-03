using System;
using System.Collections.Generic;
using NPCs;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

/// <summary>
/// Manages HUD, toasts, fade overlay, card selection UI, and dialogue UI.
/// Plain C# singleton — registered by Bootstrapper, scene references passed via Init().
/// Tick() is driven by GameDriver.Update().
/// </summary>
public class UIManager
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static UIManager Instance
    {
        get
        {
            if (DIContainer.TryGet<UIManager>(out var instance))
                return instance;
            throw new InvalidOperationException(
                "UIManager not registered. Bootstrapper should have called DIContainer.Inject().");
        }
    }

    // ── Serialized references (set via Init) ───────────────────────────────────
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

    // ── Fade state machine ────────────────────────────────────────────────────
    private enum FadeState { None, FadingToBlack, FadingFromBlack }
    private FadeState _fadeState;
    private float _fadeElapsed;
    private float _fadeDuration;

    // ── Toast state machine ───────────────────────────────────────────────────
    private enum ToastState { None, Showing, FadingOut }
    private ToastState _toastState;
    private float _toastElapsed;
    private string _pendingToast;

    /// <summary>
    /// Called by GameDriver.Awake() after scene load to supply all scene references.
    /// </summary>
    public void Init(
        Image fadeOverlay,
        TextMeshProUGUI toastText,
        GameObject dialoguePanel,
        TextMeshProUGUI npcNameText,
        TextMeshProUGUI lineText,
        GameObject cardSelectionPanel,
        Transform cardListContainer,
        GameObject cardButtonPrefab)
    {
        _fadeOverlay = fadeOverlay;
        _toastText = toastText;
        _dialoguePanel = dialoguePanel;
        _npcNameText = npcNameText;
        _lineText = lineText;
        _cardSelectionPanel = cardSelectionPanel;
        _cardListContainer = cardListContainer;
        _cardButtonPrefab = cardButtonPrefab;
    }

    // ── Tick (driven by GameDriver.Update) ────────────────────────────────────

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
        else // FadingFromBlack
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
        else // FadingOut
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

    // ── Dialogue UI ───────────────────────────────────────────────────────────

    public void HandleDialogueStarted(DialogueEntry entry, NPC _)
    {
        if (_dialoguePanel != null) _dialoguePanel.SetActive(true);
        if (_npcNameText != null) _npcNameText.text = entry.NPCDisplayName;
    }

    public void HandleDialogueEnded()
    {
        if (_dialoguePanel != null) _dialoguePanel.SetActive(false);
    }

    public void HandleLineChanged(DialogueLine line)
    {
        if (_lineText != null) _lineText.text = line.Line;
    }

    // ── Fade ──────────────────────────────────────────────────────────────────

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

    // ── Toast ─────────────────────────────────────────────────────────────────

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

    // ── Card Selection UI ─────────────────────────────────────────────────────

    public void ShowCardSelection(List<CardData> availableCards, Action<CardData> onCardSelected)
    {
        if (_cardSelectionPanel == null) return;
        _cardSelectionPanel.SetActive(true);

        // Clear previous buttons
        foreach (Transform child in _cardListContainer)
            UnityEngine.Object.Destroy(child.gameObject);

        foreach (var card in availableCards)
        {
            GameObject btnObj = UnityEngine.Object.Instantiate(_cardButtonPrefab, _cardListContainer);
            var btn = btnObj.GetComponent<Core.CardSelectionButton>();
            if (btn != null)
                btn.Setup(card, () =>
                {
                    _cardSelectionPanel.SetActive(false);
                    onCardSelected?.Invoke(card);
                });
        }
    }

    public void HideCardSelection()
    {
        if (_cardSelectionPanel != null)
            _cardSelectionPanel.SetActive(false);
    }
}
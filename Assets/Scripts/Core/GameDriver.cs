using Actions;
using Input;
using Manager.Scene;
using System.Collections.Generic;
using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Settings;
using Assets.Scripts.Puzzle;

/// <summary>
/// Thin MonoBehaviour bridge that sits in the scene and:
/// 1. Passes scene references to plain C# singletons via their Init(...) methods.
/// 2. Drives their Tick(...) methods from Unity's Update loop.
///
/// Create one GameDriver in each scene that uses managers / puzzle / dialogue.
/// Attach it to a persistent GameObject or spawn it via Bootstrapper at runtime.
/// </summary>
public class GameDriver : MonoBehaviour
{
    [Header("ProgressionManager")]
    public Transform[] spawnPoints;
    public ChapterSettings chapterSettings;

    [Header("PlayerState")]
    public CardData cardReturn;
    public List<CardData> defaultNPCCards = new();

    [Header("UIManager — Scene References")]
    public Image fadeOverlay;
    public TextMeshProUGUI toastText;
    public GameObject dialoguePanel;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI lineText;
    public GameObject cardSelectionPanel;
    public Transform cardListContainer;
    public GameObject cardButtonPrefab;

    [Header("GameplayScene")]
    public Camera playerCamera;

    [Header("Puzzle")]
    public Camera puzzleCamera;

    [Header("Action System")]
    public ActionControl playerActionControl;
    public ActionSettings actionSettingsAsset;

    private void Awake()
    {
        // ── Player FIRST (before any Init that could throw) ──────────────────
        if (playerActionControl == null)
            playerActionControl = FindAnyObjectByType<ActionControl>();

        if (playerActionControl == null)
        {
            var playerGO = new GameObject("Player");
            playerGO.tag = "Player";
            playerGO.AddComponent<CharacterController>();
            playerGO.AddComponent<InputHandle>();
            playerActionControl = playerGO.AddComponent<ActionControl>();
        }

        if (actionSettingsAsset == null)
            actionSettingsAsset = ScriptableObject.CreateInstance<ActionSettings>();

        playerActionControl.Initialize(actionSettingsAsset);

        // ── Singletons (may throw if inspector refs are missing) ─────────────
        ProgressionManager.Instance.Init(spawnPoints, chapterSettings);
        PlayerState.Instance.Init(cardReturn, defaultNPCCards);
        UIManager.Instance.Init(
            fadeOverlay, toastText, dialoguePanel, npcNameText, lineText,
            cardSelectionPanel, cardListContainer, cardButtonPrefab);
        GameplayScene.Instance.Init(playerCamera);
        Puzzle.Instance.Init(puzzleCamera);
    }

    private void OnEnable()
    {
        Puzzle.Instance.EnableInputActions();
    }

    private void OnDisable()
    {
        Puzzle.Instance.DisableInputActions();
    }

    private void Start()
    {
        // Subscribe to dialogue events (was in UIManager.Start / GameScene.Start).
        var d = Dialogue.Instance;
        d.OnDialogueStarted += UIManager.Instance.HandleDialogueStarted;
        d.OnDialogueEnded += UIManager.Instance.HandleDialogueEnded;
        d.OnLineChanged += UIManager.Instance.HandleLineChanged;

        d.OnDialogueStarted += GameplayScene.Instance.HandleDialogueStarted;
        d.OnDialogueEnded += GameplayScene.Instance.HandleDialogueEnded;
    }

    private void OnDestroy()
    {
        var d = Dialogue.Instance;
        d.OnDialogueStarted -= UIManager.Instance.HandleDialogueStarted;
        d.OnDialogueEnded -= UIManager.Instance.HandleDialogueEnded;
        d.OnLineChanged -= UIManager.Instance.HandleLineChanged;

        d.OnDialogueStarted -= GameplayScene.Instance.HandleDialogueStarted;
        d.OnDialogueEnded -= GameplayScene.Instance.HandleDialogueEnded;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        UIManager.Instance.Tick(dt);
        GameplayScene.Instance.Tick();
        Puzzle.Instance.Tick();
    }
}
using Assets.Scripts.Interaction.Actions;
using Assets.Scripts.Interaction.Puzzle;
using Assets.Scripts.Characters;
using Assets.Scripts.Progressions;
using Assets.Scripts.Settings;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Composite root MonoBehaviour — creates all services, wires dependencies,
    /// drives the game loop. The ONLY MonoBehaviour that knows about all services.
    /// No static service locator, no singleton Instance properties.
    /// </summary>
    public sealed class GameDriver : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] ProgressionSettings _progressSettings;
        [SerializeField] ActionSettings _actionSettings;
        [SerializeField] DialogueSettings _dialogSettings;

        [Header("Gui")]
        [SerializeField] GuiGameDriver _guiDriver;

        [Header("Camera")]
        [SerializeField] Camera playerCamera;
        [SerializeField] Camera puzzleCamera;

        // ── Service instances (private, no static access) ─────────────────────
        private Gui _gui;
        private Dialogue _dialogue;
        private PlayerState _playerState;
        private GameplayScene _gameplayScene;
        private Progression _progression;
        private Puzzle _puzzle;

        private readonly ActionControl _actionControl;

        private void Awake()
        {
            Assert.IsNotNull(_guiDriver);
            Assert.IsNotNull(_progressSettings);
            Assert.IsNotNull(_actionSettings);
            Assert.IsNotNull(_dialogSettings);

            // Construct all services with new T() 
            // Order: no-dependency services first, then dependent services.
            _gui = new Gui();
            _progression = new Progression();
            _puzzle = new Puzzle();
            _playerState = new PlayerState(_gui);            // depends on IGui
            _dialogue = new Dialogue(_playerState);          // depends on IPlayerState
            _gameplayScene = new GameplayScene();            // deps set via Init below

            // Wire scene refs to Gui via GuiGameDriver
            _guiDriver.WireUp(_gui, _dialogue);

            // Init leaf services (no service deps) 
            _progression.Init(_progressSettings);
            _progression.DiscoverPositionables();
            _puzzle.Init(puzzleCamera);

            // Init dependent services with explicit deps 
            _playerState.Init(_progressSettings.ReturnCard, _progressSettings.DefaultCards);
            _dialogue.Init(_dialogSettings);
            _gameplayScene.Init(playerCamera, _dialogue, _progression);

            // Wire up player actions ────────────────────────────────────
            _actionControl.Initialize(_actionSettings, _dialogue);

            // Inject service refs into scene MonoBehaviours ─────────────
            var npcs = FindObjectsByType<NPC>();
            foreach (var npc in npcs)
                npc.DialogueRef = _dialogue;

            var triggers = FindObjectsByType<PuzzleTrigger>();
            foreach (var t in triggers)
                t.PuzzleRef = _puzzle;

            var beds = FindObjectsByType<Bed>();
            foreach (var b in beds)
            {
                b.GameplaySceneRef = _gameplayScene;
                b.GuiRef = _gui;
            }

            // ── 7. Subscribe dialogue events → gameplay scene handlers ──────
            _dialogue.OnDialogueStarted += _gameplayScene.HandleDialogueStarted;
            _dialogue.OnDialogueEnded += _gameplayScene.HandleDialogueEnded;
        }

        private void Start()
        {
            // Enable puzzle input actions after all services are ready
            _puzzle.EnableInputActions();
        }

        private void OnEnable()
        {
            _puzzle.EnableInputActions();
        }

        private void OnDisable()
        {
            _puzzle.DisableInputActions();
        }

        private void OnDestroy()
        {
            // Unsubscribe event handlers to prevent leaks
            _dialogue.OnDialogueStarted -= _gameplayScene.HandleDialogueStarted;
            _dialogue.OnDialogueEnded -= _gameplayScene.HandleDialogueEnded;
        }

        private void Update()
        {
            _gameplayScene.Tick();
            _puzzle.Tick();
        }
    }
}
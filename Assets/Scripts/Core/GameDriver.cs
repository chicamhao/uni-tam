using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.Characters;
using Assets.Scripts.Interaction.Actions;
using Assets.Scripts.Interaction.Input;
using Assets.Scripts.Interaction.Puzzle;
using Assets.Scripts.Progressions;
using Assets.Scripts.Settings;
using Assets.Scripts.UI;

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
        [SerializeField] Camera _playerCamera;
        [SerializeField] Camera _puzzleCamera;

        // ── Service instances (private, no static access) ─────────────────────
        private Gui _gui;
        private Dialogue _dialogue;
        private PlayerState _playerState;
        private Director _director;
        private Progression _progression;
        private Puzzle _puzzle;

        private ActionControl _actionControl;
        private InputHandle _inputHandle;

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
            _director = new Director();            // deps set via Init below

            // Wire scene refs to Gui via GuiGameDriver
            _guiDriver.WireUp(_gui, _dialogue);

            // Init leaf services (no service deps) 
            _progression.Init(_progressSettings);
            _progression.DiscoverPositionables();
            _puzzle.Init(_puzzleCamera);

            // Init dependent services with explicit deps 
            _playerState.Init(_progressSettings.ReturnCard, _progressSettings.DefaultCards);
            _dialogue.Init(_dialogSettings);
            _director.Init(_playerCamera, _dialogue, _progression);

            // Find scene services once and inject ──────────────────────
            _actionControl = FindAnyObjectByType<ActionControl>();
            Assert.IsNotNull(_actionControl, "ActionControl must exist in the scene.");
            _inputHandle = FindAnyObjectByType<InputHandle>();

            // Wire up player actions ────────────────────────────────────
            _actionControl?.Initialize(_actionSettings, _dialogue);

            // InputHandle must exist on ActionControl's GameObject (guaranteed by [RequireComponent])
            Assert.IsNotNull(_inputHandle, "InputHandle not found after ActionControl.Initialize.");

            // Inject service refs into scene MonoBehaviours ─────────────
            var actors = FindObjectsByType<Actor>();
            foreach (var actor in actors)
            {
                var interaction = actor.GetComponent<Characters.Interaction>();
                if (interaction != null) interaction.DialogueRef = _dialogue;
            }

            var triggers = FindObjectsByType<PuzzleTrigger>();
            foreach (var t in triggers)
            {
                t.PuzzleRef = _puzzle;
                t.InputHandleRef = _inputHandle;
            }

            var beds = FindObjectsByType<Bed>();
            foreach (var b in beds)
            {
                b.DirectorRef = _director;
                b.GuiRef = _gui;
                b.InputHandleRef = _inputHandle;
            }

            var crosshairs = FindObjectsByType<CrosshairInteractor>();
            foreach (var c in crosshairs)
                c.ActionControlRef = _actionControl;

            // Inject into Director ─────────────────────────────────
            _director.InputHandleRef = _inputHandle;

            // ── Subscribe dialogue events → gameplay scene handlers ────
            _dialogue.OnDialogueStarted += _director.HandleDialogueStarted;
            _dialogue.OnDialogueEnded += _director.HandleDialogueEnded;
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
            _dialogue.OnDialogueStarted -= _director.HandleDialogueStarted;
            _dialogue.OnDialogueEnded -= _director.HandleDialogueEnded;
        }

        private void Update()
        {
            _director.Tick();
            _puzzle.Tick();
        }
    }
}
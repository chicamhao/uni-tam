using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace Assets.Scripts.Puzzle
{

    /// <summary>
    /// Manages puzzle state: selection, swapping, and exit.
    /// Plain C# singleton — registered by Bootstrapper, scene refs passed via Init().
    /// Tick() driven by GameDriver.Update().
    /// </summary>
    public sealed class Puzzle
    {
        // ── Singleton ──────────────────────────────────────────────────────────────
        public static Puzzle Instance => DIContainer.Get<Puzzle>();

        // ── Events ────────────────────────────────────────────────────────────────
        /// <summary>Fired when the puzzle becomes active.</summary>
        public event Action OnPuzzleStarted;
        /// <summary>Fired when the puzzle is exited.</summary>
        public event Action OnPuzzleExited;

        // ── State ──────────────────────────────────────────────────────────────────
        private Camera _puzzleCamera;
        private bool _isActive;
        private PuzzleObject _selectedObject;
        private readonly List<PuzzleObject> _allPuzzleObjects = new();

        // Input actions (new Input System)
        private InputAction _clickAction;
        private InputAction _exitAction;

        public string ClickActionName { get; set; } = "Player/Use";
        public string ExitActionName { get; set; } = "Player/Quit";
        private bool _actionsEnabled;

        /// <summary>
        /// Called by GameDriver.Awake() after scene load.
        /// </summary>
        public void Init(Camera puzzleCamera)
        {
            _puzzleCamera = puzzleCamera;

            // Collect all puzzle objects in the scene
            var found = UnityEngine.Object.FindObjectsByType<PuzzleObject>();
            _allPuzzleObjects.Clear();
            _allPuzzleObjects.AddRange(found);

            // Bind input actions
            _clickAction = InputSystem.actions?.FindAction(ClickActionName);
            _exitAction = InputSystem.actions?.FindAction(ExitActionName);
        }

        /// <summary>
        /// Called by GameDriver.OnEnable / OnDisable to sync input action enable state
        /// with the driver GameObject's lifecycle.
        /// </summary>
        public void EnableInputActions()
        {
            if (_actionsEnabled) return;
            _clickAction?.Enable();
            _exitAction?.Enable();
            _actionsEnabled = true;
        }

        public void DisableInputActions()
        {
            if (!_actionsEnabled) return;
            _clickAction?.Disable();
            _exitAction?.Disable();
            _actionsEnabled = false;
        }

        // ── Tick (driven by GameDriver.Update) ────────────────────────────────────

        public void Tick()
        {
            if (!_isActive) return;

            // Exit via bound action
            if (_exitAction != null && _exitAction.WasPressedThisFrame())
            {
                ExitPuzzle();
                return;
            }

            // Click detection via bound action
            if (_clickAction != null && _clickAction.WasPressedThisFrame())
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (_puzzleCamera == null) return;
                Ray ray = _puzzleCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    PuzzleObject clicked = hit.collider.GetComponent<PuzzleObject>();
                    if (clicked != null)
                        HandleObjectClick(clicked);
                }
            }
        }

        /// <summary>
        /// Activates or deactivates the puzzle.
        /// </summary>
        public void SetActive(bool active)
        {
            if (_isActive == active) return;
            _isActive = active;

            if (active)
            {
                OnPuzzleStarted?.Invoke();
            }
            else
            {
                if (_selectedObject != null)
                {
                    _selectedObject.SetSelected(false);
                    _selectedObject = null;
                }
                OnPuzzleExited?.Invoke();
            }
        }

        private void HandleObjectClick(PuzzleObject clicked)
        {
            if (_selectedObject == null)
            {
                _selectedObject = clicked;
                _selectedObject.SetSelected(true);
            }
            else if (_selectedObject == clicked)
            {
                _selectedObject.SetSelected(false);
                _selectedObject = null;
            }
            else
            {
                // Swap positions
                Vector3 tempPos = _selectedObject.transform.position;
                _selectedObject.transform.position = clicked.transform.position;
                clicked.transform.position = tempPos;

                _selectedObject.SetSelected(false);
                clicked.SetSelected(false);
                _selectedObject = null;
            }
        }

        private void ExitPuzzle()
        {
            if (_selectedObject != null)
            {
                _selectedObject.SetSelected(false);
                _selectedObject = null;
            }

            _isActive = false;
            OnPuzzleExited?.Invoke();
        }
    }
}
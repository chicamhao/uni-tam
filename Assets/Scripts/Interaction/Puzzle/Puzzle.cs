using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Interaction.Puzzle
{
    /// <summary>
    /// Plain C# puzzle system managing click-select/click-swap logic.
    /// Created by GameDriver, no static access. IPuzzle interface for loose coupling.
    /// </summary>
    public sealed class Puzzle : IPuzzle
    {
        // ── Events ──────────────────────────────────────────────────────────
        public event Action OnPuzzleStarted;
        public event Action OnPuzzleExited;

        // ── State ───────────────────────────────────────────────────────────
        private bool _isActive;
        private Camera _puzzleCamera;
        private PuzzleObject _selectedObject;
        private readonly List<PuzzleObject> _puzzleObjects = new();

        public string ClickActionName { get; set; } = "Use";
        public string ExitActionName { get; set; } = "Start";

        public void Init(Camera puzzleCamera)
        {
            _puzzleCamera = puzzleCamera;
        }

        public void EnableInputActions() { }
        public void DisableInputActions() { }

        public void Tick()
        {
            if (!_isActive) return;

            if (InputSystem.actions.FindAction("Player/Use").WasPressedThisFrame())
                HandleClick();

            if (InputSystem.actions.FindAction("Player/Start").WasPressedThisFrame())
                SetActive(false);
        }

        private void HandleClick()
        {
            var ray = _puzzleCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var puzzleObj = hit.collider.GetComponentInParent<PuzzleObject>();
                if (puzzleObj != null)
                {
                    if (_selectedObject == null)
                    {
                        _selectedObject = puzzleObj;
                        _selectedObject.SetSelected(true);
                    }
                    else if (_selectedObject == puzzleObj)
                    {
                        _selectedObject.SetSelected(false);
                        _selectedObject = null;
                    }
                    else
                    {
                        SwapObjects(_selectedObject, puzzleObj);
                        _selectedObject.SetSelected(false);
                        _selectedObject = null;
                    }
                }
            }
        }

        private void SwapObjects(PuzzleObject a, PuzzleObject b)
        {
            Vector3 tempPos = a.transform.position;
            a.transform.position = b.transform.position;
            b.transform.position = tempPos;
        }

        public void SetActive(bool active)
        {
            if (_isActive == active) return;

            _isActive = active;

            if (active)
            {
                _puzzleObjects.Clear();
                var found = UnityEngine.Object.FindObjectsByType<PuzzleObject>();
                _puzzleObjects.AddRange(found);
                _selectedObject = null;
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
    }
}
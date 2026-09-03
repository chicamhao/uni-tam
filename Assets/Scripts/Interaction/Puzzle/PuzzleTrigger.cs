using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Input;
using UnityEngine;

namespace Assets.Scripts.Interaction.Puzzle
{
    /// <summary>
    /// Trigger volume that activates the puzzle via IPuzzle.SetActive().
    /// Receives IPuzzle reference set by GameDriver on Awake().
    /// Manages camera swap, cursor state, and player input enable/disable.
    /// </summary>
    public sealed class PuzzleTrigger : MonoBehaviour
    {
        public Camera puzzleCamera;
        public Camera playerCamera;

        /// <summary>
        /// Set by GameDriver on Awake() — explicit dependency injection for MonoBehaviours.
        /// </summary>
        public IPuzzle PuzzleRef { get; set; }

        private void Start()
        {
            if (PuzzleRef != null)
            {
                PuzzleRef.OnPuzzleStarted += HandlePuzzleStarted;
                PuzzleRef.OnPuzzleExited += HandlePuzzleExited;
            }
        }

        private void OnDestroy()
        {
            if (PuzzleRef != null)
            {
                PuzzleRef.OnPuzzleStarted -= HandlePuzzleStarted;
                PuzzleRef.OnPuzzleExited -= HandlePuzzleExited;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (PuzzleRef != null)
                PuzzleRef.SetActive(true);
        }

        private void HandlePuzzleStarted()
        {
            var input = FindAnyObjectByType<InputHandle>();
            if (input != null) input.DisableInput();

            if (playerCamera != null) playerCamera.enabled = false;
            if (puzzleCamera != null) puzzleCamera.enabled = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandlePuzzleExited()
        {
            if (puzzleCamera != null) puzzleCamera.enabled = false;
            if (playerCamera != null) playerCamera.enabled = true;

            var input = FindAnyObjectByType<InputHandle>();
            if (input != null) input.EnableInput();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
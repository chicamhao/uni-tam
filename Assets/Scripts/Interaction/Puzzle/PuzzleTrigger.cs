using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Input;

namespace Assets.Scripts.Interaction.Puzzle
{
    /// <summary>
    /// Trigger volume that activates the puzzle via IPuzzle.SetActive().
    /// Receives IPuzzle reference set by GameDriver on Awake().
    /// Manages camera swap, cursor state, and player input enable/disable.
    /// </summary>
    public sealed class PuzzleTrigger : MonoBehaviour
    {
        public Camera PuzzleCamera;
        public Camera PlayerCamera;

        /// <summary>
        /// Set by GameDriver on Awake() — explicit dependency injection for MonoBehaviours.
        /// </summary>
        public IPuzzle PuzzleRef { get; set; }

        /// <summary>
        /// Set by GameDriver on Awake() — injected InputHandle.
        /// </summary>
        public InputHandle InputHandleRef { get; set; }

        private void Start()
        {
            Assert.IsNotNull(PuzzleRef, "PuzzleRef must be set by GameDriver before Start().");
            PuzzleRef.OnPuzzleStarted += HandlePuzzleStarted;
            PuzzleRef.OnPuzzleExited += HandlePuzzleExited;
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
            InputHandleRef?.DisableInput();

            if (PlayerCamera != null) PlayerCamera.enabled = false;
            if (PuzzleCamera != null) PuzzleCamera.enabled = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandlePuzzleExited()
        {
            if (PuzzleCamera != null) PuzzleCamera.enabled = false;
            if (PlayerCamera != null) PlayerCamera.enabled = true;

            InputHandleRef?.EnableInput();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
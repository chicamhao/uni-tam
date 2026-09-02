using Input;
using UnityEngine;
using Utility;

/// <summary>
/// Trigger volume that activates the puzzle via Puzzle.SetActive().
/// Subscribes to Puzzle lifecycle events (like GameplayScene subscribes to Dialogue events)
/// to handle camera swap, cursor state, and player input enable/disable.
/// </summary>
public class PuzzleTrigger : MonoBehaviour
{
    [Header("Puzzle Setup")]
    public Camera puzzleCamera;
    public Camera playerCamera;

    private Puzzle _puzzle;

    private void Start()
    {
        // Get Puzzle via DIContainer (like Dialogue.Instance)
        _puzzle = Puzzle.Instance;

        // Subscribe to lifecycle events (like GameplayScene subscribes to Dialogue.OnDialogueStarted/Ended)
        if (_puzzle != null)
        {
            _puzzle.OnPuzzleStarted += HandlePuzzleStarted;
            _puzzle.OnPuzzleExited += HandlePuzzleExited;
        }
    }

    private void OnDestroy()
    {
        if (_puzzle != null)
        {
            _puzzle.OnPuzzleStarted -= HandlePuzzleStarted;
            _puzzle.OnPuzzleExited -= HandlePuzzleExited;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Activate puzzle — event handler handles camera/cursor/input
        if (_puzzle != null)
            _puzzle.SetActive(true);
    }

    // ─── Event handlers (camera/cursor/input logic moved here from Puzzle.ExitPuzzle) ───

    private void HandlePuzzleStarted()
    {
        // Disable player input
        var input = FindAnyObjectByType<InputHandle>();
        if (input != null) input.DisableInput();

        // Swap cameras
        if (playerCamera != null) playerCamera.enabled = false;
        if (puzzleCamera != null) puzzleCamera.enabled = true;

        // Show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandlePuzzleExited()
    {
        // Restore cameras
        if (puzzleCamera != null) puzzleCamera.enabled = false;
        if (playerCamera != null) playerCamera.enabled = true;

        // Re-enable player input
        var input = FindAnyObjectByType<InputHandle>();
        if (input != null) input.EnableInput();

        // Hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
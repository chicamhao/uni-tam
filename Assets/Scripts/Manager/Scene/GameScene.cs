using Input;
using UnityEngine;
using Utility;

namespace Manager.Scene
{
    /// <summary>
    /// Top-level scene manager that holds the current chapter and orchestrates
    /// camera swap / input during dialogue.
    /// Plain C# singleton — registered by Bootstrapper, scene refs passed via Init().
    /// Tick() driven by GameDriver.Update().
    /// </summary>
    public class GameplayScene
    {
        // ── Singleton ──────────────────────────────────────────────────────────
        public static GameplayScene Instance => DIContainer.Get<GameplayScene>();

        // ── State ──────────────────────────────────────────────────────────────
        private int _currentChapter = 1;
        public int CurrentChapter
        {
            get => _currentChapter;
            set => _currentChapter = value;
        }

        private Camera _playerCamera;

        /// <summary>
        /// Called by GameDriver.Awake() after scene load.
        /// </summary>
        public void Init(Camera playerCamera)
        {
            _playerCamera = playerCamera;
        }

        // ── Tick (driven by GameDriver.Update) ────────────────────────────────

        public void Tick()
        {
            Dialogue.Instance.Update();
        }

        // ── Dialogue camera + input ───────────────────────────────────────────

        /// <summary>
        /// Called by GameDriver when dialogue starts (subscribed in GameDriver.Start).
        /// </summary>
        public void HandleDialogueStarted(DialogueEntry entry, NPC npc)
        {
            if (npc == null) return;

            // Disable player input
            var input = Object.FindAnyObjectByType<InputHandle>();
            if (input != null) input.DisableInput();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Swap to conversation camera
            if (_playerCamera != null)
                _playerCamera.enabled = false;

            if (npc.conversationCamera != null)
            {
                npc.conversationCamera.enabled = true;
                Transform head = npc.GetHeadTransform();
                if (head != null)
                    npc.conversationCamera.transform.LookAt(head);
            }
        }

        /// <summary>
        /// Called by GameDriver when dialogue ends (subscribed in GameDriver.Start).
        /// </summary>
        public void HandleDialogueEnded()
        {
            // Restore player camera
            if (_playerCamera != null)
                _playerCamera.enabled = true;

            // Disable NPC conversation cameras
            var npcs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None);
            foreach (var npc in npcs)
            {
                if (npc.conversationCamera != null)
                    npc.conversationCamera.enabled = false;
            }

            // Re-enable player input
            var input = Object.FindAnyObjectByType<InputHandle>();
            if (input != null) input.EnableInput();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ── Chapter ───────────────────────────────────────────────────────────

        public void AdvanceChapter()
        {
            _currentChapter++;
            ProgressionManager.Instance?.ApplyChapter(_currentChapter);
        }
    }
}
using Assets.Scripts.Characters;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Input;
using Assets.Scripts.Settings;
using Settings;
using UnityEngine;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Scene-level game state — camera, chapter, dialogue mode transitions.
    /// Plain C# service. Created by GameDriver, dependencies injected via Init().
    /// No static access. Tick() driven by GameDriver.
    /// </summary>
    public sealed class GameplayScene : IGameplayScene
    {
        private int _currentChapter = 1;
        public int CurrentChapter
        {
            get => _currentChapter;
            set => _currentChapter = value;
        }

        private Camera _playerCamera;
        private IDialogue _dialogue;
        private IProgression _progression;

        public void Init(Camera playerCamera, IDialogue dialogue, IProgression progression)
        {
            _playerCamera = playerCamera;
            _dialogue = dialogue;
            _progression = progression;
        }

        public void Tick()
        {
            _dialogue.Update();
        }

        // ── Dialogue camera + input ───────────────────────────────────────────

        public void HandleDialogueStarted(DialogueEntry entry, NPC npc)
        {
            if (npc == null) return;

            var input = Object.FindAnyObjectByType<InputHandle>();
            if (input != null) input.DisableInput();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

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

        public void HandleDialogueEnded()
        {
            if (_playerCamera != null)
                _playerCamera.enabled = true;

            var npcs = Object.FindObjectsByType<NPC>();
            foreach (var npc in npcs)
            {
                if (npc.conversationCamera != null)
                    npc.conversationCamera.enabled = false;
            }

            var input = Object.FindAnyObjectByType<InputHandle>();
            if (input != null) input.EnableInput();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ── Chapter ───────────────────────────────────────────────────────────

        public void AdvanceChapter()
        {
            _currentChapter++;
            _progression.ApplyChapter(_currentChapter);
        }
    }
}
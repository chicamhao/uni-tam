using UnityEngine;
using Assets.Scripts.Characters;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Input;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Directs scene-level presentation modes — camera swap, cursor/input state, and chapter progression.
    /// Plain C# service. Created by GameDriver, dependencies injected via Init().
    /// No static access. Tick() driven by GameDriver.
    /// </summary>
    public sealed class Director : IDirector
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

        /// <summary>
        /// Set by GameDriver on Awake() — injected InputHandle.
        /// </summary>
        public InputHandle InputHandleRef { get; set; }

        private Actor _activeNpc;

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


        public void HandleDialogueStarted(DialogueEntry entry, Actor npc)
        {
            _activeNpc = npc;

            InputHandleRef?.DisableInput();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _playerCamera.enabled = false;

            if (npc.ConversationCamera != null)
            {
                npc.ConversationCamera.enabled = true;
                Transform head = npc.GetHeadTransform();
                if (head != null)
                    npc.ConversationCamera.transform.LookAt(head);
            }
        }

        public void HandleDialogueEnded()
        {
            _playerCamera.enabled = true;

            if (_activeNpc.ConversationCamera != null)
                _activeNpc.ConversationCamera.enabled = false;

            _activeNpc = null;

            InputHandleRef?.EnableInput();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        public void AdvanceChapter()
        {
            _currentChapter++;
            _progression.ApplyChapter(_currentChapter);
        }
    }
}

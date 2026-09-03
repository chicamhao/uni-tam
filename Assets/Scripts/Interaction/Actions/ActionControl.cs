using Assets.Scripts.Core;
using Assets.Scripts.Interaction.Input;
using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;
using UnityEngine;

namespace Assets.Scripts.Interaction.Actions
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(InputHandle))]
    public sealed class ActionControl : MonoBehaviour, IPositionable
    {
        public ActionContext Context => _context;
        private ActionContext _context;

        private CrouchAction _crouchAction;
        private JumpAction _jumpAction;
        private MoveAction _moveAction;
        private InteractAction _interactAction;
        private SkipLineAction _skipLineAction;

        private ActionSettings _settings;
        private bool _initialized;

        /// <summary>
        /// Called by GameDriver after scene load. Accepts IDialogue for SkipLineAction.
        /// </summary>
        public void Initialize(ActionSettings settings, Assets.Scripts.Interfaces.IDialogue dialogue)
        {
            _settings = settings;

            var controller = GetComponent<CharacterController>();
            if (controller == null)
                controller = gameObject.AddComponent<CharacterController>();

            var input = GetComponent<InputHandle>();
            if (input == null)
                input = gameObject.AddComponent<InputHandle>();

            _context = new ActionContext(_settings, controller, input);

            _crouchAction = new CrouchAction(_context, _settings.Crouch);
            _jumpAction = new JumpAction(_context, _settings.Jump);
            _moveAction = new MoveAction(_context, _settings.Move, _settings.Jump, _settings.Crouch);
            _interactAction = new InteractAction(_context);
            _skipLineAction = new SkipLineAction(_context, dialogue);

            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            if (_context.Input.IsQuitActionElapsed(_settings.QuitHoldTime))
                Application.Quit();

            _context.Validate(Time.time);
            _crouchAction.Crouch();
            _moveAction.Rotate();
            _moveAction.Move();
            _jumpAction.Jump();
            _skipLineAction.Skip();
            _interactAction.Interact();
        }

        // --- IPositionable ---
        public string GetActorID() => "player";

        public void ApplyState(ChapterEntry state, Transform spawnPoint)
        {
            if (spawnPoint == null) return;

            var controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            controller.enabled = true;

            if (state.Anim != null)
            {
                var animator = GetComponent<Animator>();
                if (animator != null) animator.Play(state.Anim.name);
            }
        }
    }
}
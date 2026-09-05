using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Interaction.Input
{
    /// <summary>Binds and exposes player input actions (move, look, jump, sprint, crouch, interact, quit, skip).</summary>
    public sealed class InputHandle : MonoBehaviour
    {
        [Tooltip("Sensitivity multiplier for moving the camera around")]
        public float LookSensitivity = 1f;

        [Tooltip("Limit to consider an input when using a trigger on a controller")]
        public float TriggerAxisThreshold = 0.4f;

        [Tooltip("Used to flip the vertical input axis")]
        public bool InvertYAxis = false;

        [Tooltip("Used to flip the horizontal input axis")]
        public bool InvertXAxis = false;

        public bool InputEnabled = true;

        private bool _quitInputWasHeld;
        private float _quitHeldTime = 0f;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;
        private InputAction _useAction;
        private InputAction _interactAction;
        private InputAction _quitAction;
        private InputAction _startAction;
        private InputAction _skipAction;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            var asset = InputSystem.actions;
            if (asset == null)
            {
                Debug.LogWarning("[InputHandle] InputSystem.actions is null — no default input action asset assigned in Project Settings. Controls disabled.");
                InputEnabled = false;
                return;
            }

            BindAction(ref _moveAction, asset, "Move");
            BindAction(ref _lookAction, asset, "Look");
            BindAction(ref _jumpAction, asset, "Jump");
            BindAction(ref _sprintAction, asset, "Sprint");
            BindAction(ref _crouchAction, asset, "Crouch");
            BindAction(ref _useAction, asset, "Use");
            BindAction(ref _interactAction, asset, "Interact");
            BindAction(ref _quitAction, asset, "Quit");
            BindAction(ref _startAction, asset, "Start");
            BindAction(ref _skipAction, asset, "Skip");

            _moveAction?.Enable();
            _lookAction?.Enable();
            _jumpAction?.Enable();
            _sprintAction?.Enable();
            _crouchAction?.Enable();
            _useAction?.Enable();
            _interactAction?.Enable();
            _skipAction?.Enable();
            _quitAction?.Enable();
            _startAction?.Enable();
        }

        private void BindAction(ref InputAction action, InputActionAsset asset, string name)
        {
            action = asset.FindAction("Player/" + name);
            if (action == null)
                Debug.LogWarning($"[InputHandle] Action 'Player/{name}' not found in input actions asset.");
        }

        private void LateUpdate()
        {
            _quitInputWasHeld = GetQuitInputHeld();
        }

        public Vector3 GetMoveInput()
        {
            if (!CanProcessInput()) return Vector3.zero;

            var input = _moveAction.ReadValue<Vector2>();
            var move = new Vector3(input.x, 0f, input.y);
            move = Vector3.ClampMagnitude(move, 1);
            return move;
        }

        public float GetLookInputsHorizontal()
        {
            if (!CanProcessInput()) return 0f;
            var input = _lookAction.ReadValue<Vector2>().x;
            if (InvertXAxis) input *= -1;
            return input * LookSensitivity;
        }

        public float GetLookInputsVertical()
        {
            if (!CanProcessInput()) return 0f;
            var input = _lookAction.ReadValue<Vector2>().y;
            if (InvertYAxis) input *= -1;
            return input * LookSensitivity;
        }

        public bool GetJumpInputDown() => CanProcessInput() && _jumpAction.WasPressedThisFrame();
        public bool GetJumpInputHeld() => CanProcessInput() && _jumpAction.IsPressed();
        public bool GetSprintInputHeld() => CanProcessInput() && _sprintAction.IsPressed();
        public bool GetCrouchInputDown() => CanProcessInput() && _crouchAction.WasPressedThisFrame();
        public bool GetCrouchInputReleased() => CanProcessInput() && _crouchAction.WasReleasedThisFrame();
        public bool GetInteractInputDown() => CanProcessInput() && _interactAction.WasPressedThisFrame();
        public bool GetUseInputDown() => CanProcessInput() && _useAction != null && _useAction.WasPressedThisFrame();
        public bool GetSkipInputDown() => CanProcessInput() && _skipAction != null && _skipAction.WasPressedThisFrame();
        public bool GetQuitInputHeld() => _quitAction != null && _quitAction.IsPressed();
        public bool GetQuitInputDown() => GetQuitInputHeld() && !_quitInputWasHeld;
        public bool GetQuitInputReleased() => !GetQuitInputHeld() && _quitInputWasHeld;
        public bool GetStartInputDown() => _startAction != null && _startAction.WasPressedThisFrame();

        public bool IsQuitActionElapsed(float duration)
        {
            if (GetQuitInputHeld())
            {
                _quitHeldTime += Time.deltaTime;
                if (_quitHeldTime >= duration) return true;
            }
            else
            {
                _quitHeldTime = 0f;
            }
            return false;
        }

        private bool CanProcessInput() => InputEnabled && Cursor.lockState == CursorLockMode.Locked;

        public void DisableInput() => InputEnabled = false;
        public void EnableInput() => InputEnabled = true;
    }
}
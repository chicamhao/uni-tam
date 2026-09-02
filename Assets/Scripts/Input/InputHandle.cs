using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
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

        private float _quitHeldTime = 0f;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            BindAction(ref _moveAction, "Move");
            BindAction(ref _lookAction, "Look");
            BindAction(ref _jumpAction, "Jump");
            BindAction(ref _sprintAction, "Sprint");
            BindAction(ref _crouchAction, "Crouch");
            BindAction(ref _useAction, "Use");
            BindAction(ref _interactAction, "Interact");
            BindAction(ref _quitAction, "Quit");
            BindAction(ref _startAction, "Start");
            BindAction(ref _skipAction, "Skip");

            _moveAction.Enable();
            _lookAction.Enable();
            _jumpAction.Enable();
            _sprintAction.Enable();
            _crouchAction.Enable();
            _useAction.Enable();
            _interactAction.Enable();
            _skipAction.Enable();
            _quitAction.Enable();
            _startAction.Enable();
        }

        void BindAction(ref InputAction action, string name)
        {
            action = InputSystem.actions.FindAction("Player/" + name);
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
            input *= LookSensitivity;
            return input;
        }

        public float GetLookInputsVertical()
        {
            if (!CanProcessInput()) return 0f;

            var input = _lookAction.ReadValue<Vector2>().y;
            if (InvertYAxis) input *= -1;
            input *= LookSensitivity;
            return input;
        }

        public bool GetJumpInputDown()
        {
            return CanProcessInput() && _jumpAction.WasPressedThisFrame();
        }

        public bool GetJumpInputHeld()
        {
            return CanProcessInput() && _jumpAction.IsPressed();
        }

        public bool GetSprintInputHeld()
        {
            return CanProcessInput() && _sprintAction.IsPressed();
        }

        public bool GetCrouchInputDown()
        {
            return CanProcessInput() && _crouchAction.WasPressedThisFrame();
        }

        public bool GetCrouchInputReleased()
        {
            return CanProcessInput() && _crouchAction.WasReleasedThisFrame();
        }

        public bool GetInteractInputDown()
        {
            return CanProcessInput() && _interactAction.WasPressedThisFrame();
        }

        public bool GetUseInputDown()
        {
            return CanProcessInput() && _useAction.WasPressedThisFrame();
        }

        public bool GetSkipInputDown()
        {
            return CanProcessInput() && _skipAction.WasPressedThisFrame();
        }

        public bool GetQuitInputHeld()
        {
            return _quitAction.IsPressed();
        }

        public bool GetQuitInputDown()
        {
            return GetQuitInputHeld() && !_quitInputWasHeld;
        }

        public bool GetQuitInputReleased()
        {
            return !GetQuitInputHeld() && _quitInputWasHeld;
        }

        public bool GetStartInputDown()
        {
            return _startAction.WasPressedThisFrame();
        }

        public bool IsQuitActionElapsed(float duration)
        {
            if (GetQuitInputHeld())
            {
                _quitHeldTime += Time.deltaTime;
                if (_quitHeldTime >= duration)
                {
                    return true;
                }
            }
            else
            {
                _quitHeldTime = 0f;
            }
            return false;
        }

        private bool CanProcessInput()
        {
            return InputEnabled && Cursor.lockState == CursorLockMode.Locked;
        }

        public void DisableInput()
        {
            InputEnabled = false;
        }

        public void EnableInput()
        {
            InputEnabled = true;
        }
    }
}
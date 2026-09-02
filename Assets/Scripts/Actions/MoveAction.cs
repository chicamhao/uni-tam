using Settings;
using UnityEngine;
using Utility;

namespace Actions
{
    public sealed class MoveAction
    {
        readonly MoveSettings _moveSettings;
        readonly JumpSettings _jumpSettings;
        readonly CrouchSettings _crouchSettings;
        readonly ActionContext _context;

        private float _cameraVerticalAngle = 0f;

        public MoveAction(ActionContext context,
            MoveSettings moveSettings, JumpSettings jumpSettings, CrouchSettings crouchSettings)
        {
            _context = context;
            _moveSettings = moveSettings;
            _jumpSettings = jumpSettings;
            _crouchSettings = crouchSettings;
        }

        public void Rotate()
        {
            var rotateSpeed = _moveSettings.RotationSpeed;

            // horizontal character rotation
            {
                _context.Controller.transform.Rotate(
                    new Vector3(0f, _context.Input.GetLookInputsHorizontal() * rotateSpeed, 0f), Space.Self);
            }

            // vertical camera rotation
            {
                _cameraVerticalAngle += _context.Input.GetLookInputsVertical() * rotateSpeed;
                _cameraVerticalAngle = Mathf.Clamp(_cameraVerticalAngle, -89f, 89f);
                Camera.main.transform.localEulerAngles = new Vector3(_cameraVerticalAngle, 0, 0);
            }
        }

        public void Move()
        {
            var speedModifier = 1.0f;

            if (!_context.IsCrouching && _context.Input.GetSprintInputHeld())
            {
                speedModifier = _moveSettings.SprintSpeedModifier;
            }

            var velocity = _context.IsGrounded
                ? GetGroundVelocity(speedModifier, _context.IsCrouching, _context)
                : GetAirVelocity(speedModifier, _context);

            // apply the final calculated velocity value as a character movement.
            var capsuleBottomBeforeMove = Calculator.GetCapsuleBottomHemisphere(_context.Controller);
            var capsuleTopBeforeMove = Calculator.GetCapsuleTopHemisphere(_context.Controller);

            _context.Controller.Move(velocity * Time.deltaTime);

            // detect obstructions to adjust velocity accordingly.
            if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, _context.Controller.radius,
                velocity.normalized, out var hit, velocity.magnitude * Time.deltaTime))
            {
                velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
            }

            _context.Velocity = velocity;
        }

        private Vector3 GetGroundVelocity(float speedModifier, bool isCrouching, ActionContext context)
        {
            var worldSpaceMoveInput = _context.Controller.transform.TransformVector(_context.Input.GetMoveInput());

            var targetVelocity = worldSpaceMoveInput * (_moveSettings.MaxSpeedOnGround * speedModifier);

            if (isCrouching)
            {
                targetVelocity *= _crouchSettings.MaxSpeedCrouchedRatio;
            }

            targetVelocity = Calculator.GetDirectionReorientedOnSlope(
                targetVelocity.normalized, context.GroundNormal, _context.Controller.transform.up) * targetVelocity.magnitude;

            return Vector3.Lerp(context.Velocity, targetVelocity, _moveSettings.MovementSharpnessOnGround * Time.deltaTime);
        }

        private Vector3 GetAirVelocity(float speedModifier, ActionContext context)
        {
            var velocity = context.Velocity;

            var worldSpaceMoveInput = _context.Controller.transform.TransformVector(_context.Input.GetMoveInput());

            velocity += worldSpaceMoveInput * (_jumpSettings.AccelerationSpeedInAir * Time.deltaTime);

            var verticalVelocity = velocity.y;
            var horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, _jumpSettings.MaxSpeedInAir * speedModifier);
            velocity = horizontalVelocity + (Vector3.up * verticalVelocity);

            velocity += Vector3.down * (_jumpSettings.GravityDownForce * Time.deltaTime);
            return velocity;
        }
    }
}
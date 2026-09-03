using Assets.Scripts.Interaction.Input;
using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;
using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Interaction.Actions
{
    public sealed class ActionContext
    {
        private static readonly float _groundDistance = 1f;
        private static readonly float _groundDistanceInAir = 0.07f;
        private static readonly float _jumpGroundingPreventionTime = 0.2f;

        public bool IsGrounded { get; set; }
        public bool IsCrouching { get; set; }
        public float LastTimeJumped { get; set; }
        public Vector3 GroundNormal { get; set; }
        public Vector3 Velocity { get; set; }

        public IInteractable InteractObject { get; set; }

        public CharacterController Controller { get; }
        private readonly CharacterController _controller;

        public InputHandle Input { get; }
        private readonly InputHandle _input;

        public ActionSettings Settings { get; }
        private readonly ActionSettings _settings;

        public ActionContext(ActionSettings settings, CharacterController controller, InputHandle input)
        {
            _settings = settings;
            _controller = controller;
            _input = input;
        }

        public void Validate(float time)
        {
            if (!(time >= LastTimeJumped + _jumpGroundingPreventionTime))
                return;

            var chosenGroundCheckDistance = IsGrounded ? (_controller.skinWidth + _groundDistance) : _groundDistanceInAir;

            bool wasGrounded = IsGrounded;
            IsGrounded = false;
            GroundNormal = Vector3.up;

            if (Physics.CapsuleCast(
                    Calculator.GetCapsuleBottomHemisphere(_controller),
                    Calculator.GetCapsuleTopHemisphere(_controller),
                    _controller.radius, Vector3.down, out var hit, chosenGroundCheckDistance))
            {
                GroundNormal = hit.normal;

                if (Vector3.Dot(GroundNormal, _controller.transform.up) > 0f
                    && Vector3.Angle(_controller.transform.up, GroundNormal) < _controller.slopeLimit)
                {
                    IsGrounded = true;
                    if (hit.distance < _controller.skinWidth)
                        _controller.Move(Vector3.down * hit.distance);
                }
            }
        }
    }
}
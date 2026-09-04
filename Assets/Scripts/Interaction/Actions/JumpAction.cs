using UnityEngine;
using Assets.Scripts.Settings;
using Assets.Scripts.Context;

namespace Assets.Scripts.Interaction.Actions
{
    /// <summary>Applies upward velocity to the player when grounded and jump input is triggered.</summary>
    public sealed class JumpAction
    {
        private readonly ActionContext _context;
        private readonly JumpSettings _jumpSettings;

        public JumpAction(ActionContext context, JumpSettings jumpSettings)
        {
            _context = context;
            _jumpSettings = jumpSettings;
        }

        public void Jump()
        {
            if (!_context.Input.GetJumpInputDown()) return;
            if (!_context.IsGrounded) return;

            _context.LastTimeJumped = Time.time;
            _context.Velocity = new Vector3(
                _context.Velocity.x,
                _jumpSettings.JumpForce,
                _context.Velocity.z);
        }
    }
}
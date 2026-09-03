using Assets.Scripts.Settings;
using UnityEngine;

namespace Assets.Scripts.Interaction.Actions
{
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
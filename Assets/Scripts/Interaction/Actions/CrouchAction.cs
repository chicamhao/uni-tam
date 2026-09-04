using UnityEngine;
using Assets.Scripts.Settings;
using Assets.Scripts.Utility;
using Assets.Scripts.Context;

namespace Assets.Scripts.Interaction.Actions
{
    /// <summary>Handles player crouch state transitions and capsule height changes.</summary>
    public sealed class CrouchAction
    {
        private readonly ActionContext _context;
        private readonly CrouchSettings _crouchSettings;

        public CrouchAction(ActionContext context, CrouchSettings crouchSettings)
        {
            _context = context;
            _crouchSettings = crouchSettings;
        }

        public void Crouch()
        {
            var controller = _context.Controller;

            if (_context.Input.GetCrouchInputDown())
            {
                if (_context.IsCrouching)
                {
                    // Try to stand up
                    if (Calculator.Standable(_context, _crouchSettings.CapsuleHeightStanding))
                    {
                        controller.height = _crouchSettings.CapsuleHeightStanding;
                        _context.IsCrouching = false;
                    }
                }
                else
                {
                    controller.height = _crouchSettings.CapsuleHeightCrouching;
                    _context.IsCrouching = true;
                }
            }
        }
    }
}
using Settings;
using UnityEngine;
using Utility;

namespace Actions
{
    public sealed class CrouchAction
    {
        readonly ActionContext _context;
        readonly CrouchSettings _settings;

        public CrouchAction(ActionContext context, CrouchSettings settings)
        {
            _context = context;
            _settings = settings;
            ForceUpdateHeight();
        }

        public void Crouch()
        {
            if (_context.Input.GetCrouchInputDown())
            {
                if (_context.IsCrouching)
                {
                    bool crouchable = Calculator.Standable(_context, _settings.CapsuleHeightStanding);
                    if (crouchable)
                    {
                        _context.IsCrouching = false;
                    }
                }
                else
                {
                    _context.IsCrouching = true;
                }
            }
            UpdateHeight();
        }

        private void UpdateHeight()
        {
            var height = _context.IsCrouching
                ? _settings.CapsuleHeightCrouching
                : _settings.CapsuleHeightStanding;

            if (Mathf.Approximately(_context.Controller.height, height)) return;

            _context.Controller.height = Mathf.Lerp(
                _context.Controller.height, height, _settings.CrouchingSharpness * Time.deltaTime);

            _context.Controller.center = Vector3.up * (_context.Controller.height * 0.5f);

            Camera.main.transform.localPosition = Vector3.Lerp(Camera.main.transform.localPosition,
                Vector3.up * (height * _settings.CameraHeightRatio), _settings.CrouchingSharpness * Time.deltaTime);
        }

        private void ForceUpdateHeight()
        {
            _context.Controller.height = _settings.CapsuleHeightStanding;
            _context.Controller.center = Vector3.up * (_context.Controller.height * 0.5f);
            Camera.main.transform.localPosition = Vector3.up * (_context.Controller.height * _settings.CameraHeightRatio);
        }
    }
}
using Assets.Scripts.Interaction.Actions;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class Calculator
    {
        private static readonly Collider[] _colliders = new Collider[3];

        public static Vector3 GetCapsuleBottomHemisphere(CharacterController controller)
        {
            return controller.transform.position + (controller.transform.up * controller.radius);
        }

        public static Vector3 GetCapsuleTopHemisphere(CharacterController controller, float height)
        {
            return controller.transform.position + (controller.transform.up * (height - controller.radius));
        }

        public static Vector3 GetCapsuleTopHemisphere(CharacterController controller)
        {
            return GetCapsuleTopHemisphere(controller, controller.height);
        }

        public static Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal, Vector3 up)
        {
            var directionRight = Vector3.Cross(direction, up);
            return Vector3.Cross(slopeNormal, directionRight).normalized;
        }

        public static bool Standable(ActionContext context, float height)
        {
            _colliders[0] = null;
            _colliders[1] = null;
            _colliders[2] = null;

            Physics.OverlapCapsuleNonAlloc(
                GetCapsuleBottomHemisphere(context.Controller),
                GetCapsuleTopHemisphere(context.Controller, height),
                context.Controller.radius, _colliders);

            foreach (Collider c in _colliders)
            {
                if (c != null && c != context.Controller && !c.isTrigger)
                    return false;
            }
            return true;
        }
    }
}
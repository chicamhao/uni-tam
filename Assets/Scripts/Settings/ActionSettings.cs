using System;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    /// <summary>ScriptableObject holding all player action settings (move, jump, crouch, interact, stamina).</summary>
    [CreateAssetMenu(fileName = "ActionSettings", menuName = "ScriptableObjects/ActionSettings", order = 1)]
    public sealed class ActionSettings : ScriptableObject
    {
        public MoveSettings Move;
        public JumpSettings Jump;
        public CrouchSettings Crouch;
        public InteractSettings Interact;
        public StaminaSettings Stamina;

        [Tooltip("Time in seconds the quit action needs to be held to quit the game")]
        public float QuitHoldTime = 3f;
    }

    /// <summary>Settings for player ground movement speed, rotation, sprint, and sharpness.</summary>
    [Serializable]
    public sealed class MoveSettings
    {
        [Tooltip("Rotation speed for moving the camera")]
        public float RotationSpeed = 200f;

        [Tooltip("Max movement speed when grounded (when not sprinting)")]
        public float MaxSpeedOnGround = 13f;

        [Tooltip("Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
        public float MovementSharpnessOnGround = 15f;

        [Tooltip("Multiplication for the sprint speed (based on grounded speed)")]
        public float SprintSpeedModifier = 1.5f;

        [Range(0f, 1f)]
        public float PickableModifierCoefficient = 0.1f;
    }

    /// <summary>Settings for player jump force, air speed, acceleration, and gravity.</summary>
    [Serializable]
    public sealed class JumpSettings
    {
        [Tooltip("Vertical speed")]
        public float JumpForce = 9f;

        [Tooltip("Max movement speed when not grounded")]
        public float MaxSpeedInAir = 25f;

        [Tooltip("Acceleration speed when in the air")]
        public float AccelerationSpeedInAir = 25f;

        [Tooltip("Force applied downward when in the air")]
        public float GravityDownForce = 12f;

        [Tooltip("Height of character when crouching")]
        public float CapsuleHeightStanding = 1.8f;
    }

    /// <summary>Settings for player crouch capsule height, camera ratio, and speed reduction.</summary>
    [Serializable]
    public sealed class CrouchSettings
    {
        [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
        public float CameraHeightRatio = 0.9f;

        [Tooltip("Speed of crouching transitions")]
        public float CrouchingSharpness = 10f;

        [Tooltip("Height of character when standing")]
        public float CapsuleHeightStanding = 1.8f;

        [Tooltip("Height of character when crouching")]
        public float CapsuleHeightCrouching = 0.9f;

        [Tooltip("Max movement speed when crouching")]
        [Range(0, 1)]
        public float MaxSpeedCrouchedRatio = 0.5f;
    }

    /// <summary>Settings for player interaction with objects (e.g. throw force).</summary>
    [Serializable]
    public sealed class InteractSettings
    {
        public float ThrowForce = 10f;
    }

    /// <summary>Settings for player stamina — max, regen rate, consumption rate, and regen delay.</summary>
    [Serializable]
    public sealed class StaminaSettings
    {
        [Tooltip("Maximum stamina the player can have")]
        public float MaxStamina = 100f;

        [Tooltip("Stamina regeneration rate per second when not sprinting")]
        public float StaminaRegenRate = 20f;

        [Tooltip("Stamina consumption rate per second when sprinting")]
        public float StaminaConsumptionRate = 30f;

        [Tooltip("Delay in seconds before stamina starts regenerating after sprinting stops")]
        public float StaminaRegenDelay = 1f;
    }
}
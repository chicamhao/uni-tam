using System.Collections;
using UnityEngine;
using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Characters
{
    /// <summary>Base actor component for any entity positioned by the chapter system (player, NPCs).</summary>
    [DisallowMultipleComponent]
    public sealed class Actor : MonoBehaviour, IPositionable
    {
        [Header("Identity")]
        [SerializeField] private ActorIdentifier _identifier;

        /// <summary>Unique identifier matching DialogueSettings entries.</summary>
        public string ActorID => _identifier.ActorID;
        /// <summary>Display name shown in UI prompts and toasts.</summary>
        public string DisplayName => _identifier.DisplayName;
        public ActorIdentifier Identifier => _identifier;

        [Header("Dialogue Camera")]
        [SerializeField] private Camera _conversationCamera;
        /// <summary>Camera used for conversation close-ups (null if actor has no dedicated camera).</summary>
        public Camera ConversationCamera => _conversationCamera;

        /// <summary>Returns the head bone transform for conversation camera aiming (null if not an NPC with Animator).</summary>
        public Transform GetHeadTransform()
        {
            var animator = GetComponent<Animator>();
            return animator != null ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
        }


        public string GetActorID() => _identifier.ActorID;

        public void ApplyState(ChapterEntry state, Transform spawnPoint)
        {
            var materialHandle = GetComponent<MaterialHandle>();
            if (materialHandle != null)
                StartCoroutine(ApplyStateWithFade(state, spawnPoint, materialHandle));
            else
                ApplyStateImmediate(state, spawnPoint);
        }

        private void ApplyStateImmediate(ChapterEntry state, Transform spawnPoint)
        {
            if (spawnPoint == null) return;

            var controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            if (controller != null) controller.enabled = true;

            if (state.Anim != null)
            {
                var animator = GetComponent<Animator>();
                if (animator != null) animator.Play(state.Anim.name);
            }
        }

        private IEnumerator ApplyStateWithFade(ChapterEntry state, Transform spawnPoint, MaterialHandle materialHandle)
        {
            bool isVisible = state.IsVisible != false;

            yield return materialHandle.FadeAlpha(1f, 0f, 0.5f);

            if (spawnPoint != null)
            {
                transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }

            if (state.Anim != null)
            {
                var animator = GetComponent<Animator>();
                if (animator != null) animator.Play(state.Anim.name);
            }

            if (!isVisible)
                gameObject.SetActive(false);
            else
                yield return materialHandle.FadeAlpha(0f, 1f, 0.5f);
        }
    }
}

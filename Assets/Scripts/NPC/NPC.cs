using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;
using Settings;
using UnityEngine;

namespace Assets.Scripts.Characters
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public sealed class NPC : MonoBehaviour, IInteractable, IPositionable
    {
        [Header("Identity")]
        public string NPCID;
        public string DisplayName;

        [Header("References")]
        public Camera conversationCamera;
        public SkinnedMeshRenderer skinnedMeshRenderer;

        /// <summary>
        /// Set by GameDriver on Awake() — explicit dependency injection for MonoBehaviours.
        /// </summary>
        public IDialogue DialogueRef { get; set; }

        private MaterialPropertyBlock _mpb;
        private bool _mpbInitialized;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Returns the head bone transform for conversation camera aiming.
        /// </summary>
        public Transform GetHeadTransform()
        {
            if (_animator != null)
                return _animator.GetBoneTransform(HumanBodyBones.Head);
            return null;
        }

        // --- IPositionable ---
        public string GetActorID() => NPCID;

        public void ApplyState(ChapterEntry state, Transform spawnPoint)
        {
            if (state.IsVisible == false)
                StartCoroutine(FadeOutAndTeleport(state, spawnPoint));
            else
                StartCoroutine(FadeOutTeleportFadeIn(state, spawnPoint));
        }

        private IEnumerator FadeOutAndTeleport(ChapterEntry state, Transform spawnPoint)
        {
            yield return FadeAlpha(1f, 0f, 0.5f);

            if (spawnPoint != null)
                transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            if (state.Anim != null && _animator != null)
                _animator.Play(state.Anim.name);

            gameObject.SetActive(false);
        }

        private IEnumerator FadeOutTeleportFadeIn(ChapterEntry state, Transform spawnPoint)
        {
            yield return FadeAlpha(1f, 0f, 0.5f);

            if (spawnPoint != null)
                transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            if (state.Anim != null && _animator != null)
                _animator.Play(state.Anim.name);

            yield return FadeAlpha(0f, 1f, 0.5f);
        }

        private IEnumerator FadeAlpha(float from, float to, float duration)
        {
            if (skinnedMeshRenderer == null) yield break;

            GetMPB();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(from, to, t);
                Color c = skinnedMeshRenderer.sharedMaterial.color;
                c.a = alpha;
                _mpb.SetColor("_BaseColor", c);
                _mpb.SetColor("_Color", c);
                skinnedMeshRenderer.SetPropertyBlock(_mpb);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Color final = skinnedMeshRenderer.sharedMaterial.color;
            final.a = to;
            _mpb.SetColor("_BaseColor", final);
            _mpb.SetColor("_Color", final);
            skinnedMeshRenderer.SetPropertyBlock(_mpb);
        }

        private void GetMPB()
        {
            if (!_mpbInitialized)
            {
                _mpb = new MaterialPropertyBlock();
                skinnedMeshRenderer.GetPropertyBlock(_mpb);
                _mpbInitialized = true;
            }
        }

        // --- IInteractable ---
        public void Interact()
        {
            if (DialogueRef != null)
                DialogueRef.OpenCardSelectionForNPC(this);
        }

        public string GetPrompt() => $"Talk to {DisplayName}";

        public void SetHighlight(bool highlighted)
        {
            GetMPB();
            _mpb.SetFloat("_Intensity", highlighted ? 1f : 0f);
            skinnedMeshRenderer.SetPropertyBlock(_mpb);
        }

        // --- Facial Expressions ---
        public void ApplyExpression(ExpressionDefinition expression)
        {
            if (expression == null || skinnedMeshRenderer == null) return;

            StartCoroutine(BlendToExpression(expression.MorphTargets));
        }

        private IEnumerator BlendToExpression(List<MorphTargetValue> morphTargets)
        {
            if (skinnedMeshRenderer == null) yield break;

            int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
            var activeMorphs = new HashSet<string>();
            foreach (var mt in morphTargets)
                activeMorphs.Add(mt.name);

            for (int i = 0; i < blendShapeCount; i++)
            {
                string shapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
                if (!activeMorphs.Contains(shapeName))
                    skinnedMeshRenderer.SetBlendShapeWeight(i, 0f);
            }

            foreach (var mt in morphTargets)
            {
                int index = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(mt.name);
                if (index < 0) continue;

                float startWeight = skinnedMeshRenderer.GetBlendShapeWeight(index);
                float targetWeight = mt.value * 100f;

                float elapsed = 0f;
                float duration = mt.blendInTime;
                while (elapsed < duration)
                {
                    float t = elapsed / duration;
                    skinnedMeshRenderer.SetBlendShapeWeight(index, Mathf.Lerp(startWeight, targetWeight, t));
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                skinnedMeshRenderer.SetBlendShapeWeight(index, targetWeight);
            }
        }
    }
}
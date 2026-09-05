using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Assets.Scripts.Settings;

namespace Assets.Scripts.Characters
{
    /// <summary>Handles facial expression blend-shape animation on NPC models.</summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [DisallowMultipleComponent]
    public sealed class ExpressionHandle : MonoBehaviour
    {
        private SkinnedMeshRenderer _renderer;

        private void Awake() => _renderer = GetComponent<SkinnedMeshRenderer>();

        /// <summary>Blends the renderer toward the given expression over the morph targets' blend-in times.</summary>
        public void ApplyExpression(ExpressionDefinition expression)
        {
            if (expression == null) return;
            Assert.IsNotNull(_renderer);
            StartCoroutine(BlendToExpression(expression.MorphTargets));
        }

        private IEnumerator BlendToExpression(List<MorphTargetValue> morphTargets)
        {
            Assert.IsNotNull(_renderer);

            int blendShapeCount = _renderer.sharedMesh.blendShapeCount;
            var activeMorphs = new HashSet<string>();
            foreach (var mt in morphTargets)
                activeMorphs.Add(mt.name);

            for (int i = 0; i < blendShapeCount; i++)
            {
                string shapeName = _renderer.sharedMesh.GetBlendShapeName(i);
                if (!activeMorphs.Contains(shapeName))
                    _renderer.SetBlendShapeWeight(i, 0f);
            }

            foreach (var mt in morphTargets)
            {
                int index = _renderer.sharedMesh.GetBlendShapeIndex(mt.name);
                if (index < 0) continue;

                float startWeight = _renderer.GetBlendShapeWeight(index);
                float targetWeight = mt.value * 100f;

                float elapsed = 0f;
                float duration = mt.blendInTime;
                while (elapsed < duration)
                {
                    float t = elapsed / duration;
                    _renderer.SetBlendShapeWeight(index, Mathf.Lerp(startWeight, targetWeight, t));
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                _renderer.SetBlendShapeWeight(index, targetWeight);
            }
        }
    }
}

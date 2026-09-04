using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Scripts.Characters
{
    /// <summary>Owns MaterialPropertyBlock for skinned mesh fade and highlight on NPC models.</summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    [DisallowMultipleComponent]
    public sealed class MaterialHandle : MonoBehaviour
    {
        private MaterialPropertyBlock _mpb;
        private bool _mpbInitialized;
        private SkinnedMeshRenderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<SkinnedMeshRenderer>();
            Assert.IsNotNull(_renderer);
        }

        private void EnsureMPB()
        {
            if (!_mpbInitialized)
            {
                _mpb = new MaterialPropertyBlock();
                _renderer.GetPropertyBlock(_mpb);
                _mpbInitialized = true;
            }
        }

        /// <summary>Fades the renderer alpha from `from` to `to` over `duration` seconds.</summary>
        public IEnumerator FadeAlpha(float from, float to, float duration)
        {
            EnsureMPB();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(from, to, t);
                Color c = _renderer.sharedMaterial.color;
                c.a = alpha;
                _mpb.SetColor("_BaseColor", c);
                _mpb.SetColor("_Color", c);
                _renderer.SetPropertyBlock(_mpb);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Color final = _renderer.sharedMaterial.color;
            final.a = to;
            _mpb.SetColor("_BaseColor", final);
            _mpb.SetColor("_Color", final);
            _renderer.SetPropertyBlock(_mpb);
        }

        /// <summary>Toggles highlight intensity on the renderer.</summary>
        public void SetHighlight(bool highlighted, float intensity = 1f)
        {
            EnsureMPB();
            _mpb.SetFloat("_Intensity", highlighted ? intensity : 0f);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}

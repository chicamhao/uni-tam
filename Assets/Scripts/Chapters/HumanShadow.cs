using System.Collections;
using UnityEngine;

namespace Chapters
{
    /// <summary>
    /// A human shadow with pulsing opacity. Starts disabled, enabled after footprints complete.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class HumanShadow : MonoBehaviour
    {
        [Header("Opacity Animation")]
        public float pulseSpeed = 1f;
        public float intensity = 0.3f;
        public float baseOpacity = 0.5f;

        [Header("Material")]
        public string opacityProperty = "_Opacity";

        private MaterialPropertyBlock _mpb;
        private Renderer _rend;
        private float _time;

        private void Awake()
        {
            _rend = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            _rend.GetPropertyBlock(_mpb);
        }

        private void OnEnable()
        {
            _time = 0f;
        }

        private void Update()
        {
            _time += Time.deltaTime;
            float opacity = Mathf.Sin(_time * pulseSpeed) * intensity + baseOpacity;
            opacity = Mathf.Clamp01(opacity);

            _mpb.SetFloat(opacityProperty, opacity);
            _rend.SetPropertyBlock(_mpb);
        }

        public void EnableWithDelay(float delay = 0f)
        {
            StartCoroutine(EnableRoutine(delay));
        }

        private IEnumerator EnableRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            gameObject.SetActive(true);
        }

        public void FadeOut(float duration = 1f, System.Action onComplete = null)
        {
            StartCoroutine(FadeOutRoutine(duration, onComplete));
        }

        private IEnumerator FadeOutRoutine(float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            _rend.GetPropertyBlock(_mpb);
            float start = _mpb.GetFloat(opacityProperty);

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float opacity = Mathf.Lerp(start, 0f, t);
                _mpb.SetFloat(opacityProperty, opacity);
                _rend.SetPropertyBlock(_mpb);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _mpb.SetFloat(opacityProperty, 0f);
            _rend.SetPropertyBlock(_mpb);
            gameObject.SetActive(false);

            onComplete?.Invoke();
        }
    }
}
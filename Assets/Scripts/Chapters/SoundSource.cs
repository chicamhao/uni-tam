using System.Collections;
using Interfaces;
using UnityEngine;

namespace Chapters
{
    /// <summary>
    /// A looping audio source that grows in volume over time and fades with distance. Implements IInteractable.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundSource : MonoBehaviour, IInteractable
    {
        [Header("Audio")]
        public AudioClip clip;
        public float maxVolume = 1f;
        public float maxHearDistance = 50f;

        [Header("Grow")]
        public float growDuration = 3f;

        [Header("Interaction")]
        public float fadeOutDuration = 1.5f;

        [Header("Highlight")]
        public Renderer highlightRenderer;

        private AudioSource _audioSource;
        private float _elapsedTime;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.volume = 0f;
            if (clip != null) _audioSource.clip = clip;
            _audioSource.Play();

            if (highlightRenderer != null)
            {
                _mpb = new MaterialPropertyBlock();
                highlightRenderer.GetPropertyBlock(_mpb);
            }
        }

        private void Update()
        {
            if (_audioSource == null) return;

            _elapsedTime += Time.deltaTime;

            // Distance from player camera
            float distance = float.MaxValue;
            if (Camera.main != null)
                distance = Vector3.Distance(transform.position, Camera.main.transform.position);

            float timeAlpha = Mathf.Clamp01(_elapsedTime / growDuration);
            float distAlpha = 1f - Mathf.Clamp01(distance / maxHearDistance);
            float volume = timeAlpha * distAlpha * maxVolume;

            // Apply with smoothing
            _audioSource.volume = Mathf.Lerp(_audioSource.volume, volume, Time.deltaTime * 4f);
        }

        // --- IInteractable ---
        public void Interact()
        {
            // Fade out and stop
            StartCoroutine(FadeOutAndStop());
        }

        public string GetPrompt()
        {
            return "Investigate sound source";
        }

        public void SetHighlight(bool highlighted)
        {
            if (highlightRenderer == null || _mpb == null) return;
            _mpb.SetFloat("_Intensity", highlighted ? 1f : 0f);
            highlightRenderer.SetPropertyBlock(_mpb);
        }

        private IEnumerator FadeOutAndStop()
        {
            float startVolume = _audioSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                float t = elapsed / fadeOutDuration;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _audioSource.volume = 0f;
            _audioSource.Stop();
        }
    }
}
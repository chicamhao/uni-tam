using UnityEngine;

namespace Assets.Scripts.FX
{
    /// <summary>
    /// Ambient sound source with configurable play mode and range.
    /// </summary>
    public sealed class SoundSource : MonoBehaviour
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public float range = 10f;
        public bool loop = false;
        public bool playOnStart = true;

        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.maxDistance = range;
            _audioSource.spatialBlend = 1f;
            _audioSource.loop = loop;
            _audioSource.playOnAwake = false;

            if (playOnStart && clip != null)
                _audioSource.Play();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            if (_audioSource != null && clip != null)
                _audioSource.Play();
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            if (_audioSource != null)
                _audioSource.Stop();
        }
    }
}
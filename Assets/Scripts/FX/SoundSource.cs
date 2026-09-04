using UnityEngine;

namespace Assets.Scripts.FX
{
    /// <summary>
    /// Ambient sound source with configurable play mode and range.
    /// </summary>
    public sealed class SoundSource : MonoBehaviour
    {
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
        public float Range = 10f;
        public bool Loop = false;
        public bool PlayOnStart = true;

        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.clip = Clip;
            _audioSource.volume = Volume;
            _audioSource.maxDistance = Range;
            _audioSource.spatialBlend = 1f;
            _audioSource.loop = Loop;
            _audioSource.playOnAwake = false;

            if (PlayOnStart && Clip != null)
                _audioSource.Play();
        }

        [ContextMenu("Play")]
        public void Play()
        {
            if (_audioSource != null && Clip != null)
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
using System;
using UnityEngine;

namespace Utility
{
    /// <summary>Counts elapsed time against a duration and fires completion on update; implements IDisposable.</summary>
    public sealed class Timer : IDisposable
    {
        private float _elapsedTime;
        public float ElapsedTime => _elapsedTime;
        public float T => _elapsedTime / _duration;

        private float _duration = -1;
        public float Duration => _duration;

        public bool IsValid() => _duration >= 0;

        public override string ToString() => _elapsedTime.ToString("F1") + "/" + _duration.ToString("F1");

        public Timer() { }

        public Timer(float duration)
        {
            SetDuration(duration);
        }

        public void SetDuration(float duration)
        {
            Dispose();
            _duration = duration;
        }

        public bool Update()
        {
            if (IsValid())
            {
                _elapsedTime += Time.deltaTime;
                return _elapsedTime >= _duration;
            }
            return false;
        }

        public void Dispose()
        {
            _duration = -1;
            _elapsedTime = 0;
        }
    }
}
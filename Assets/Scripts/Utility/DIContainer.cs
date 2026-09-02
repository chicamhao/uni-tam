using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    /// <summary>
    /// Generic singleton container. Every <typeparamref name="T"/> gets its own
    /// static cache slot. Call <see cref="Inject{T}"/> once per type to register,
    /// then retrieve via <see cref="Get{T}"/> or <see cref="TryGet{T}"/>.
    /// </summary>
    public sealed class DIContainer
    {
        /// <summary>
        /// Tracks all active registrations for clean disposal at domain reload / play-mode exit.
        /// </summary>
        private static readonly List<IDisposable> _allRegistrations = new();

        public static IDisposable Inject<T>(T instance)
        {
            var token = Cache<T>.Register(instance);
            _allRegistrations.Add(token);
            return token;
        }

        public static T Get<T>()
        {
            return Cache<T>.Get();
        }

        public static bool TryGet<T>(out T instance)
        {
            instance = Cache<T>.s_cache != null
                ? Cache<T>.s_cache._instance
                : default;
            return Cache<T>.s_cache != null;
        }

        /// <summary>
        /// Clears the static tracking list. Called by Bootstrapper on domain reload
        /// to prevent stale references.
        /// </summary>
        public static void ResetTracking()
        {
            _allRegistrations.Clear();
        }

        private sealed class Cache<T> : IDisposable
        {
            internal static Cache<T> s_cache;
            internal readonly T _instance;

            private Cache(T instance)
            {
                _instance = instance;
            }

            internal static IDisposable Register(T instance)
            {
                if (s_cache != null)
                {
                    Debug.LogWarning(
                        $"[DIContainer] {typeof(T).Name} is already registered. " +
                        "Disposing previous registration first.");
                    s_cache.Dispose();
                }

                s_cache = new Cache<T>(instance);
                return s_cache;
            }

            internal static T Get()
            {
                if (s_cache == null)
                    throw new InvalidOperationException(
                        $"[DIContainer] {typeof(T).Name} has not been registered yet. " +
                        "Call DIContainer.Inject() first.");
                return s_cache._instance;
            }

            public void Dispose()
            {
                s_cache = null;
            }
        }
    }
}
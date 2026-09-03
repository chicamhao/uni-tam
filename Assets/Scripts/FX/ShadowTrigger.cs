using Assets.Scripts.Progressions;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.FX
{
    /// <summary>
    /// Trigger volume that activates shadow-related FX when the player enters.
    /// Receives service references set by GameDriver.
    /// </summary>
    public sealed class ShadowTrigger : MonoBehaviour
    {
        public GameObject shadowFXPrefab;
        public float activationDelay = 0.5f;

        public IGameplayScene GameplaySceneRef { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (shadowFXPrefab != null)
                Instantiate(shadowFXPrefab, transform.position, Quaternion.identity);
        }
    }
}
using UnityEngine;

namespace Assets.Scripts.FX
{
    /// <summary>
    /// Trigger volume that activates shadow-related FX when the player enters.
    /// Receives service references set by GameDriver.
    /// </summary>
    public sealed class ShadowTrigger : MonoBehaviour
    {
        public GameObject ShadowFXPrefab;
        public float ActivationDelay = 0.5f;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (ShadowFXPrefab != null)
                Instantiate(ShadowFXPrefab, transform.position, Quaternion.identity);
        }
    }
}
using System.Collections;
using UnityEngine;

/// <summary>
/// Trigger that fades out the HumanShadow and fires the next event.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ShadowTrigger : MonoBehaviour
{
    [Header("Target")]
    public HumanShadow shadow;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    [Header("Next Event")]
    public UnityEngine.Events.UnityEvent OnShadowFaded;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (shadow != null)
        {
            shadow.FadeOut(fadeDuration, () =>
            {
                OnShadowFaded?.Invoke();
            });
        }
        else
        {
            OnShadowFaded?.Invoke();
        }

        // Disable trigger so it only fires once
        GetComponent<Collider>().enabled = false;
    }
}
using System.Collections;
using UnityEngine;

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

    private AudioSource audioSource;
    private float elapsedTime;
    private MaterialPropertyBlock mpb;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0f;
        if (clip != null) audioSource.clip = clip;
        audioSource.Play();

        if (highlightRenderer != null)
        {
            mpb = new MaterialPropertyBlock();
            highlightRenderer.GetPropertyBlock(mpb);
        }
    }

    private void Update()
    {
        if (audioSource == null) return;

        elapsedTime += Time.deltaTime;

        // Distance from player camera
        float distance = float.MaxValue;
        if (Camera.main != null)
            distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        float timeAlpha = Mathf.Clamp01(elapsedTime / growDuration);
        float distAlpha = 1f - Mathf.Clamp01(distance / maxHearDistance);
        float volume = timeAlpha * distAlpha * maxVolume;

        // Apply with smoothing
        audioSource.volume = Mathf.Lerp(audioSource.volume, volume, Time.deltaTime * 4f);
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
        if (highlightRenderer == null || mpb == null) return;
        mpb.SetFloat("_Intensity", highlighted ? 1f : 0f);
        highlightRenderer.SetPropertyBlock(mpb);
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            float t = elapsed / fadeOutDuration;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}
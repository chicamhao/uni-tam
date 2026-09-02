using System.Collections;
using UnityEngine;

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

    private MaterialPropertyBlock mpb;
    private Renderer rend;
    private float time;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        rend.GetPropertyBlock(mpb);
    }

    private void OnEnable()
    {
        // Reset time when enabled
        time = 0f;
    }

    private void Update()
    {
        time += Time.deltaTime;
        float opacity = Mathf.Sin(time * pulseSpeed) * intensity + baseOpacity;
        opacity = Mathf.Clamp01(opacity);

        mpb.SetFloat(opacityProperty, opacity);
        rend.SetPropertyBlock(mpb);
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

    /// <summary>
    /// Fades out the shadow and invokes the callback when complete.
    /// </summary>
    public void FadeOut(float duration = 1f, System.Action onComplete = null)
    {
        StartCoroutine(FadeOutRoutine(duration, onComplete));
    }

    private IEnumerator FadeOutRoutine(float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        rend.GetPropertyBlock(mpb);
        float start = mpb.GetFloat(opacityProperty);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float opacity = Mathf.Lerp(start, 0f, t);
            mpb.SetFloat(opacityProperty, opacity);
            rend.SetPropertyBlock(mpb);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mpb.SetFloat(opacityProperty, 0f);
        rend.SetPropertyBlock(mpb);
        gameObject.SetActive(false);

        onComplete?.Invoke();
    }
}
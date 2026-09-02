using UnityEngine;

/// <summary>
/// Clickable puzzle object that can be selected and swapped.
/// </summary>
public class PuzzleObject : MonoBehaviour, IClickable
{
    [Header("Visual")]
    public Renderer highlightRenderer;
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1f;

    private MaterialPropertyBlock mpb;
    private bool isSelected;

    private void Awake()
    {
        mpb = new MaterialPropertyBlock();
        if (highlightRenderer != null)
            highlightRenderer.GetPropertyBlock(mpb);
    }

    public void OnClick()
    {
        // Handled by Puzzle
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (highlightRenderer == null) return;

        highlightRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Intensity", selected ? highlightIntensity : 0f);
        if (selected)
            mpb.SetColor("_BaseColor", highlightColor);
        else
            mpb.SetColor("_BaseColor", Color.white);
        highlightRenderer.SetPropertyBlock(mpb);
    }
}
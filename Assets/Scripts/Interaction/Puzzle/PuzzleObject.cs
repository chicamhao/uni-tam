using UnityEngine;
using Assets.Scripts.Interaction.Interfaces;

namespace Assets.Scripts.Interaction.Puzzle
{
    /// <summary>
    /// Clickable puzzle object that can be selected and swapped.
    /// </summary>
    public sealed class PuzzleObject : MonoBehaviour, IClickable
    {
        public Renderer HighlightRenderer;
        public Color highlightColor = Color.yellow;
        public float highlightIntensity = 1f;

        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            if (HighlightRenderer != null)
                HighlightRenderer.GetPropertyBlock(_mpb);
        }

        public void OnClick()
        {
            // Click handled by Puzzle system via raycast
        }

        public void SetSelected(bool selected)
        {
            if (HighlightRenderer == null) return;

            HighlightRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Intensity", selected ? highlightIntensity : 0f);
            _mpb.SetColor("_BaseColor", selected ? highlightColor : Color.white);
            HighlightRenderer.SetPropertyBlock(_mpb);
        }
    }
}
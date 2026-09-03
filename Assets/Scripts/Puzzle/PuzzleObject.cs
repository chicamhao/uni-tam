using Interfaces;
using UnityEngine;

namespace Assets.Scripts.Puzzle
{
    /// <summary>
    /// Clickable puzzle object that can be selected and swapped.
    /// </summary>
    public class PuzzleObject : MonoBehaviour, IClickable
    {
        public Renderer highlightRenderer;
        public Color highlightColor = Color.yellow;
        public float highlightIntensity = 1f;

        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
            if (highlightRenderer != null)
                highlightRenderer.GetPropertyBlock(_mpb);
        }

        public void OnClick()
        {
            // Handled by Puzzle
        }

        public void SetSelected(bool selected)
        {
            if (highlightRenderer == null) return;

            highlightRenderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Intensity", selected ? highlightIntensity : 0f);
            if (selected)
                _mpb.SetColor("_BaseColor", highlightColor);
            else
                _mpb.SetColor("_BaseColor", Color.white);
            highlightRenderer.SetPropertyBlock(_mpb);
        }
    }
}
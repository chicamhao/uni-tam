using System.Collections;
using UnityEngine;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Input;
using Assets.Scripts.Interaction.Interfaces;

namespace Assets.Scripts.Progressions
{
    /// <summary>
    /// Bed interactable that triggers the sleep → wake sequence in chapter 2.
    /// Receives service references set by GameDriver on Awake().
    /// </summary>
    public sealed class Bed : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        public float SleepDuration = 8f;

        [Header("Highlight")]
        public Renderer HighlightRenderer;

        /// <summary>
        /// Set by GameDriver on Awake().
        /// </summary>
        public IDirector DirectorRef { get; set; }

        /// <summary>
        /// Set by GameDriver on Awake().
        /// </summary>
        public IGui GuiRef { get; set; }

        /// <summary>
        /// Set by GameDriver on Awake() — injected InputHandle.
        /// </summary>
        public InputHandle InputHandleRef { get; set; }

        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            if (HighlightRenderer != null)
            {
                _mpb = new MaterialPropertyBlock();
                HighlightRenderer.GetPropertyBlock(_mpb);
            }
        }

        // --- IInteractable ---
        public void Interact()
        {
            if (DirectorRef == null || DirectorRef.CurrentChapter != 2)
            {
                GuiRef?.ShowToast("Not the right time to sleep...");
                return;
            }

            StartCoroutine(SleepSequence());
        }

        public string GetPrompt() => "Sleep (Chapter 2)";

        public void SetHighlight(bool highlighted)
        {
            if (HighlightRenderer == null || _mpb == null) return;
            _mpb.SetFloat("_Intensity", highlighted ? 1f : 0f);
            HighlightRenderer.SetPropertyBlock(_mpb);
        }

        private IEnumerator SleepSequence()
        {
            DisableInput();

            GuiRef.FadeToBlack(1.5f);
            yield return new WaitForSeconds(1.5f);

            yield return new WaitForSeconds(1.5f);

            yield return new WaitForSeconds(SleepDuration);

            DirectorRef.AdvanceChapter();

            yield return new WaitForSeconds(0.3f);

            GuiRef.FadeFromBlack(1.5f);
            yield return new WaitForSeconds(1.5f);

            yield return new WaitForSeconds(1.5f);

            EnableInput();
        }

        private void DisableInput()
        {
            InputHandleRef?.DisableInput();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void EnableInput()
        {
            InputHandleRef?.EnableInput();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

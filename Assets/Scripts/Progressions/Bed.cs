using System.Collections;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Interaction.Input;
using Assets.Scripts.Interaction.Interfaces;
using UnityEngine;

namespace Assets.Scripts.Progressions
{
    /// <summary>
    /// Bed interactable that triggers the sleep → wake sequence in chapter 2.
    /// Receives service references set by GameDriver on Awake().
    /// </summary>
    public sealed class Bed : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        public float sleepDuration = 8f;

        [Header("Highlight")]
        public Renderer highlightRenderer;

        /// <summary>
        /// Set by GameDriver on Awake().
        /// </summary>
        public IGameplayScene GameplaySceneRef { get; set; }

        /// <summary>
        /// Set by GameDriver on Awake().
        /// </summary>
        public IGui GuiRef { get; set; }

        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            if (highlightRenderer != null)
            {
                _mpb = new MaterialPropertyBlock();
                highlightRenderer.GetPropertyBlock(_mpb);
            }
        }

        // --- IInteractable ---
        public void Interact()
        {
            if (GameplaySceneRef == null || GameplaySceneRef.CurrentChapter != 2)
            {
                GuiRef?.ShowToast("Not the right time to sleep...");
                return;
            }

            StartCoroutine(SleepSequence());
        }

        public string GetPrompt() => "Sleep (Chapter 2)";

        public void SetHighlight(bool highlighted)
        {
            if (highlightRenderer == null || _mpb == null) return;
            _mpb.SetFloat("_Intensity", highlighted ? 1f : 0f);
            highlightRenderer.SetPropertyBlock(_mpb);
        }

        private IEnumerator SleepSequence()
        {
            DisableInput();

            GuiRef?.FadeToBlack(1.5f);
            yield return new WaitForSeconds(1.5f);

            yield return new WaitForSeconds(1.5f);

            yield return new WaitForSeconds(sleepDuration);

            GameplaySceneRef?.AdvanceChapter();

            yield return new WaitForSeconds(0.3f);

            GuiRef?.FadeFromBlack(1.5f);
            yield return new WaitForSeconds(1.5f);

            yield return new WaitForSeconds(1.5f);

            EnableInput();
        }

        private void DisableInput()
        {
            var input = FindAnyObjectByType<InputHandle>();
            if (input != null) input.DisableInput();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void EnableInput()
        {
            var input = FindAnyObjectByType<InputHandle>();
            if (input != null) input.EnableInput();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
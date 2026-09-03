using System.Collections;
using System.Threading.Tasks;
using Input;
using Interfaces;
using Manager.Scene;
using UnityEngine;

namespace Assets.Scripts.Chapters
{
    /// <summary>
    /// Bed interactable that triggers the sleep → wake sequence in chapter 2.
    /// </summary>
    public class Bed : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        public float sleepDuration = 8f; // in-game seconds to simulate sleep

        [Header("Highlight")]
        public Renderer highlightRenderer;

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
            // Only works in chapter 2
            if (GameplayScene.Instance.CurrentChapter != 2)
            {
                UIManager.Instance?.ShowToast("Not the right time to sleep...");
                return;
            }

            StartCoroutine(SleepSequence());
        }

        public string GetPrompt()
        {
            return "Sleep (Chapter 2)";
        }

        public void SetHighlight(bool highlighted)
        {
            if (highlightRenderer == null || _mpb == null) return;
            _mpb.SetFloat("_Intensity", highlighted ? 1f : 0f);
            highlightRenderer.SetPropertyBlock(_mpb);
        }

        private IEnumerator SleepSequence()
        {
            var ui = UIManager.Instance;
            var gm = GameplayScene.Instance;

            // Disable input
            DisableInput();

            // Fade to black
            ui.FadeToBlack(1.5f);
            yield return new WaitForSeconds(1.5f);

            // Wait (1.5s)
            yield return new WaitForSeconds(1.5f);

            // Play wake sound (placeholder)
            // AudioSource.PlayClipAtPoint(wakeSound, transform.position);

            // Wait for sleep duration (simulated time)
            yield return new WaitForSeconds(sleepDuration);

            // Advance chapter
            gm.AdvanceChapter();

            // Brief wait
            yield return new WaitForSeconds(0.3f);

            // Fade from black
            ui.FadeFromBlack(1.5f);
            yield return new WaitForSeconds(1.5f);

            // Wait
            yield return new WaitForSeconds(1.5f);

            // Enable input
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
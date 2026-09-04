using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Context;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Scene-ref provider for Gui + Dialogue event wiring.
    /// Receives IGui and IDialogue via GameDriver.WireUpGui() on Awake.
    /// Drives Gui.Tick() from Update().
    /// </summary>
    public sealed class GuiGameDriver : MonoBehaviour
    {
        [Header("Gui — Scene References")]
        public Image FadeOverlay;
        public TextMeshProUGUI ToastText;
        public GameObject DialoguePanel;
        public TextMeshProUGUI NpcNameText;
        public TextMeshProUGUI LineText;
        public GameObject CardSelectionPanel;
        public Transform CardListContainer;
        public GameObject CardButtonPrefab;

        private IGui _gui;
        private IDialogue _dialogue;

        /// <summary>
        /// Called by GameDriver on Awake() to inject dependencies and wire scene refs.
        /// </summary>
        public void WireUp(IGui gui, IDialogue dialogue)
        {
            _gui = gui;
            _dialogue = dialogue;

            // Bundle scene references into GuiContext and pass to Gui service
            var config = new GuiContext(FadeOverlay, ToastText, DialoguePanel, NpcNameText, LineText,
                                       CardSelectionPanel, CardListContainer, CardButtonPrefab);
            _gui.Init(config);

            // Subscribe dialogue events → Gui handlers
            _dialogue.OnDialogueStarted += _gui.HandleDialogueStarted;
            _dialogue.OnDialogueEnded += _gui.HandleDialogueEnded;
            _dialogue.OnLineChanged += _gui.HandleLineChanged;
        }

        private void OnDestroy()
        {
            if (_dialogue != null)
            {
                _dialogue.OnDialogueStarted -= _gui.HandleDialogueStarted;
                _dialogue.OnDialogueEnded -= _gui.HandleDialogueEnded;
                _dialogue.OnLineChanged -= _gui.HandleLineChanged;
            }
        }

        private void Update()
        {
            _gui?.Tick(Time.deltaTime);
        }
    }
}
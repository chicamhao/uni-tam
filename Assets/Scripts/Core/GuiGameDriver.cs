using Assets.Scripts.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public Image fadeOverlay;
        public TextMeshProUGUI toastText;
        public GameObject dialoguePanel;
        public TextMeshProUGUI npcNameText;
        public TextMeshProUGUI lineText;
        public GameObject cardSelectionPanel;
        public Transform cardListContainer;
        public GameObject cardButtonPrefab;

        private IGui _gui;
        private IDialogue _dialogue;

        /// <summary>
        /// Called by GameDriver on Awake() to inject dependencies and wire scene refs.
        /// </summary>
        public void WireUp(IGui gui, IDialogue dialogue)
        {
            _gui = gui;
            _dialogue = dialogue;

            // Pass scene references to Gui service
            _gui.Init(fadeOverlay, toastText, dialoguePanel, npcNameText, lineText,
                      cardSelectionPanel, cardListContainer, cardButtonPrefab);

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
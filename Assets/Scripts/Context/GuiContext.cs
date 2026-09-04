using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Context
{
    /// <summary>
    /// Bundles all scene references Gui needs.
    /// </summary>
    public sealed class GuiContext
    {
        public Image FadeOverlay { get; }
        public TextMeshProUGUI ToastText { get; }
        public GameObject DialoguePanel { get; }
        public TextMeshProUGUI NpcNameText { get; }
        public TextMeshProUGUI LineText { get; }
        public GameObject CardSelectionPanel { get; }
        public Transform CardListContainer { get; }
        public GameObject CardButtonPrefab { get; }

        public GuiContext(
            Image fadeOverlay,
            TextMeshProUGUI toastText,
            GameObject dialoguePanel,
            TextMeshProUGUI npcNameText,
            TextMeshProUGUI lineText,
            GameObject cardSelectionPanel,
            Transform cardListContainer,
            GameObject cardButtonPrefab)
        {
            FadeOverlay = fadeOverlay;
            ToastText = toastText;
            DialoguePanel = dialoguePanel;
            NpcNameText = npcNameText;
            LineText = lineText;
            CardSelectionPanel = cardSelectionPanel;
            CardListContainer = cardListContainer;
            CardButtonPrefab = cardButtonPrefab;
        }
    }
}

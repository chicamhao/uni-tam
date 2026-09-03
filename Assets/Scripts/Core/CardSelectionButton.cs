using Settings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    /// <summary>
    /// UI button used in the card selection panel.
    /// </summary>
    public class CardSelectionButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _button;

        private CardData _cachedCard;

        public void Setup(CardData card, System.Action onClick)
        {
            _cachedCard = card;
            _nameText.text = card.DisplayName;
            _descText.text = card.Description;
            if (card.Icon != null)
                _iconImage.sprite = Sprite.Create(card.Icon, new Rect(0, 0, card.Icon.width, card.Icon.height), Vector2.zero);
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }

        public CardData GetCard() => _cachedCard;
    }
}
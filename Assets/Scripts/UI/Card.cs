using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Settings;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// UI button used in the card selection panel.
    /// </summary>
    public sealed class Card : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _button;

        private CardDefinition _cachedCard;

        public void Setup(CardDefinition card, System.Action onClick)
        {
            _cachedCard = card;
            if (_nameText != null) _nameText.text = card.DisplayName;
            if (_descText != null) _descText.text = card.Description;
            if (_iconImage != null && card.Icon != null)
                _iconImage.sprite = Sprite.Create(card.Icon, new Rect(0, 0, card.Icon.width, card.Icon.height), Vector2.zero);
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick?.Invoke());
        }

        public CardDefinition GetCard() => _cachedCard;
    }
}
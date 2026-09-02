using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI button used in the card selection panel.
/// </summary>
public class CardSelectionButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button button;

    private CardData cachedCard;

    public void Setup(CardData card, System.Action onClick)
    {
        cachedCard = card;
        nameText.text = card.DisplayName;
        descText.text = card.Description;
        if (card.Icon != null)
            iconImage.sprite = Sprite.Create(card.Icon, new Rect(0, 0, card.Icon.width, card.Icon.height), Vector2.zero);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public CardData GetCard() => cachedCard;
}
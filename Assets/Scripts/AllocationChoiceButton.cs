using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllocationChoiceButton : MonoBehaviour
{
    [SerializeField] private AllocationItemData itemData;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject placedMark;

    [Header("UI Labels")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI variantText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI lowMoodPriceText;
    [SerializeField] private GameObject lockedMark;

    private SystemScreenController owner;

    public string ItemId => itemData != null ? itemData.itemId : "";

    public void Setup(SystemScreenController screen)
    {
        owner = screen;

        if (iconImage != null && itemData != null)
            iconImage.sprite = itemData.icon;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
        }

        RefreshVisual();
    }

    private void OnClicked()
    {
        if (owner == null || itemData == null) return;
        owner.OnChoiceClicked(itemData);
    }

    public void RefreshVisual()
    {
        if (itemData == null) return;

        string baseName = itemData.itemName;
        string variant = "";
        int displayCost = itemData.cost;
        bool available = true;

        bool showLowMoodText = false;
        string lowMoodLabel = "";

        if (owner != null)
        {
            var eval = owner.EvaluateChoice(itemData);
            baseName = eval.baseName;
            variant = eval.variantLabel;
            displayCost = eval.finalCost;
            available = eval.isAvailable;

            showLowMoodText = eval.showLowMoodPriceText;
            lowMoodLabel = eval.lowMoodPriceLabel;
        }

        if (itemNameText != null)
            itemNameText.text = baseName;

        if (variantText != null)
        {
            variantText.text = variant;
            variantText.gameObject.SetActive(!string.IsNullOrEmpty(variant));
        }

        if (priceText != null)
            priceText.text = displayCost <= 0 ? "FREE" : $"₱{displayCost}";

        if (lowMoodPriceText != null)
        {
            lowMoodPriceText.text = lowMoodLabel;
            lowMoodPriceText.gameObject.SetActive(showLowMoodText);
        }

        if (lockedMark != null)
            lockedMark.SetActive(!available);

        if (button != null)
            button.interactable = available;
    }

    public void SetPlacedState(bool placed, bool locked)
    {
        if (placedMark != null)
            placedMark.SetActive(placed);

        bool available = true;
        if (owner != null && itemData != null)
            available = owner.EvaluateChoice(itemData).isAvailable;

        if (button != null)
            button.interactable = !locked && available;
    }

    public void SetLocked(bool locked)
    {
        bool available = true;
        if (owner != null && itemData != null)
            available = owner.EvaluateChoice(itemData).isAvailable;

        if (button != null)
            button.interactable = !locked && available;
    }
}
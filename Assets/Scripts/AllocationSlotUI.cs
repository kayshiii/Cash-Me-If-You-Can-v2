using UnityEngine;
using UnityEngine.UI;

public class AllocationSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Button slotButton;
    [SerializeField] private GameObject filledVisual;
    [SerializeField] private GameObject emptyVisual;

    private AllocationItemData currentItem;
    private SystemScreenController owner;
    private bool isLocked = false;

    public bool IsEmpty => currentItem == null;
    public bool HasItem => currentItem != null;
    public string CurrentItemId => currentItem != null ? currentItem.itemId : "";
    public int CurrentCost => currentItem != null ? currentItem.cost : 0;

    private void Awake()
    {
        SetIconAlpha(0f);

        if (filledVisual != null)
            filledVisual.SetActive(false);

        if (emptyVisual != null)
            emptyVisual.SetActive(true);
    }

    public void SetItem(AllocationItemData item, SystemScreenController screen)
    {
        currentItem = item;
        owner = screen;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            SetIconAlpha(1f);
        }

        if (filledVisual != null)
            filledVisual.SetActive(true);

        if (emptyVisual != null)
            emptyVisual.SetActive(false);

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (iconImage != null)
        {
            SetIconAlpha(0f);
        }

        if (filledVisual != null)
            filledVisual.SetActive(false);

        if (emptyVisual != null)
            emptyVisual.SetActive(true);

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
        }
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (slotButton != null)
            slotButton.interactable = !locked;
    }

    private void OnSlotClicked()
    {
        if (isLocked || currentItem == null || owner == null) return;
        owner.OnPlacedItemClicked(currentItem.itemId);
    }

    private void SetIconAlpha(float alpha)
    {
        if (iconImage == null) return;

        Color c = iconImage.color;
        c.a = alpha;
        iconImage.color = c;
    }
}
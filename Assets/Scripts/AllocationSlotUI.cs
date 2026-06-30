using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AllocationSlotUI : MonoBehaviour
{
    public enum SlotType
    {
        Lunch,
        Commute,
        Want
    }

    [SerializeField] private SlotType slotType;
    [SerializeField] private Image displayImage;
    [SerializeField] private Sprite emptySprite;

    private AllocationItemData currentItem;
    private SystemScreenController owner;
    private Button slotButton;
    private bool isLocked = false;

    public SlotType Type => slotType;
    public bool HasItem => currentItem != null;
    public string CurrentItemId => currentItem != null ? currentItem.itemId : "";
    public int CurrentCost => currentItem != null ? currentItem.cost : 0;
    public AllocationItemData CurrentItemData => currentItem;

    private void Awake()
    {
        slotButton = GetComponent<Button>();

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(OnSlotClicked);
        }

        ShowEmpty();
    }

    public void SetOwner(SystemScreenController screen)
    {
        owner = screen;
    }

    public void SetItem(AllocationItemData item, SystemScreenController screen)
    {
        currentItem = item;
        owner = screen;

        Debug.Log("SetItem called for slot: " + slotType + " | item: " + item.itemId, this);
        Debug.Log("Sprite assigned: " + (item.icon != null ? item.icon.name : "NULL"), this);

        if (displayImage != null)
        {
            displayImage.sprite = item.icon;
            displayImage.SetNativeSize(); // optional, only if you want to confirm visual change
        }
        else
        {
            Debug.LogWarning("Display Image is not assigned on slot: " + slotType, this);
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        ShowEmpty();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (slotButton != null)
            slotButton.interactable = !locked;
    }

    private void OnSlotClicked()
    {
        if (isLocked || owner == null) return;
        owner.OnSlotPressed(this);
    }

    private void ShowEmpty()
    {
        if (displayImage != null)
            displayImage.sprite = emptySprite;
    }
}
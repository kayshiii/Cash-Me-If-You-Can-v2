using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AllocationChoiceButton : MonoBehaviour
{
    [SerializeField] private AllocationItemData itemData;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject placedMark;

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
    }

    private void OnClicked()
    {
        Debug.Log("Clicked choice: " + itemData.itemName);

        if (owner == null || itemData == null) return;
        owner.OnChoiceClicked(itemData);
    }

    public void SetPlacedState(bool placed, bool locked)
    {
        if (placedMark != null)
            placedMark.SetActive(placed);

        if (button != null)
            button.interactable = !locked;
    }

    public void SetLocked(bool locked)
    {
        if (button != null) button.interactable = !locked;
    }
}
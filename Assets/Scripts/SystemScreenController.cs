using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SystemScreenController : MonoBehaviour
{
    [Header("Tabs")]
    [SerializeField] private GameObject needsSubTab;
    [SerializeField] private GameObject wantsSubTab;
    [SerializeField] private GameObject needsChoicesRoot;
    [SerializeField] private GameObject wantsChoicesRoot;

    [Header("Tab Visuals")]
    [SerializeField] private Image needsButtonImage;
    [SerializeField] private Image wantsButtonImage;
    [SerializeField] private Color activeTabColor = Color.green;
    [SerializeField] private Color inactiveTabColor = Color.white;

    [Header("Tab Buttons")]
    [SerializeField] private Button needsButton;
    [SerializeField] private Button wantsButton;
    [SerializeField] private Button confirmButton;

    [Header("Need Slots")]
    [SerializeField] private AllocationSlotUI lunchSlot;
    [SerializeField] private AllocationSlotUI commuteSlot;

    [Header("Want Slots")]
    [SerializeField] private AllocationSlotUI[] wantSlots;

    [Header("Budget Info UI")]
    [SerializeField] private TextMeshProUGUI budgetMethodText;
    [SerializeField] private TextMeshProUGUI budgetBudgetText;   // single budget text

    [Header("Savings UI")]
    [SerializeField] private TextMeshProUGUI savingsText;
    [SerializeField] private Slider savingsSlider;
    [SerializeField] private int savingsGoal = 8500;
    private int todaySavings = 0;

    [Header("Choices")]
    [SerializeField] private AllocationChoiceButton[] choiceButtons;

    private bool isConfirmed = false;

    private void Start()
    {
        foreach (var btn in choiceButtons)
        {
            if (btn != null) btn.Setup(this);
        }

        OpenNeedsTab();
        RefreshAllChoices();
        RefreshBudgetUI();
        LoadTodayAllocation();
    }

    public void OpenNeedsTab()
    {
        if (needsSubTab != null) needsSubTab.SetActive(true);
        if (wantsSubTab != null) wantsSubTab.SetActive(false);
        if (needsChoicesRoot != null) needsChoicesRoot.SetActive(true);
        if (wantsChoicesRoot != null) wantsChoicesRoot.SetActive(false);

        UpdateTabVisuals(true);
    }

    public void OpenWantsTab()
    {
        if (needsSubTab != null) needsSubTab.SetActive(false);
        if (wantsSubTab != null) wantsSubTab.SetActive(true);
        if (needsChoicesRoot != null) needsChoicesRoot.SetActive(false);
        if (wantsChoicesRoot != null) wantsChoicesRoot.SetActive(true);

        UpdateTabVisuals(false);
    }

    private void UpdateTabVisuals(bool needsActive)
    {
        if (needsButtonImage != null)
            needsButtonImage.color = needsActive ? activeTabColor : inactiveTabColor;

        if (wantsButtonImage != null)
            wantsButtonImage.color = needsActive ? inactiveTabColor : activeTabColor;
    }

    public void OnChoiceClicked(AllocationItemData item)
    {
        if (isConfirmed) return;

        if (IsItemPlaced(item.itemId))
        {
            RemoveItem(item.itemId);
            RefreshBudgetUI();
            RefreshAllChoices();
            SaveTodayAllocation();
            return;
        }

        switch (item.category)
        {
            case AllocationCategory.NeedLunch:
                PlaceIntoSlot(lunchSlot, item);
                break;

            case AllocationCategory.NeedCommute:
                PlaceIntoSlot(commuteSlot, item);
                break;

            case AllocationCategory.Want:
                PlaceIntoFirstEmptyWantSlot(item);
                break;
        }

        RefreshBudgetUI();
        RefreshAllChoices();
        SaveTodayAllocation();
    }

    public void OnPlacedItemClicked(string itemId)
    {
        if (isConfirmed) return;

        RemoveItem(itemId);
        RefreshBudgetUI();
        RefreshAllChoices();
        SaveTodayAllocation();
    }

    public void OnConfirmClicked()
    {
        if (isConfirmed) return;

        RefreshBudgetUI();

        isConfirmed = true;

        int needsSpent = 0;
        int wantsSpent = 0;

        if (lunchSlot != null && lunchSlot.HasItem)
            needsSpent += lunchSlot.CurrentCost;

        if (commuteSlot != null && commuteSlot.HasItem)
            needsSpent += commuteSlot.CurrentCost;

        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.HasItem)
                wantsSpent += slot.CurrentCost;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ConfirmTodayAllocation(
                GetCurrentSpent(),
                GetCurrentSelectedItemIds()
            );

            GameManager.Instance.SetTodayCategorySpent(needsSpent, wantsSpent);
            GameManager.Instance.SetTodaySaved(todaySavings);
            GameManager.Instance.AddSavings(todaySavings);
        }

        if (confirmButton != null) confirmButton.interactable = false;
        if (needsButton != null) needsButton.interactable = false;
        if (wantsButton != null) wantsButton.interactable = false;

        foreach (var btn in choiceButtons)
        {
            if (btn != null) btn.SetLocked(true);
        }

        LockAllSlots(true);
    }

    private void PlaceIntoSlot(AllocationSlotUI slot, AllocationItemData item)
    {
        if (slot == null) return;
        slot.SetItem(item, this);
        RefreshAllChoices();
    }

    private void PlaceIntoFirstEmptyWantSlot(AllocationItemData item)
    {
        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.IsEmpty)
            {
                slot.SetItem(item, this);
                RefreshAllChoices();
                return;
            }
        }
    }

    private void RemoveItem(string itemId)
    {
        if (lunchSlot != null && lunchSlot.CurrentItemId == itemId)
            lunchSlot.ClearSlot();

        if (commuteSlot != null && commuteSlot.CurrentItemId == itemId)
            commuteSlot.ClearSlot();

        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.CurrentItemId == itemId)
                slot.ClearSlot();
        }
    }

    private bool IsItemPlaced(string itemId)
    {
        if (lunchSlot != null && lunchSlot.CurrentItemId == itemId) return true;
        if (commuteSlot != null && commuteSlot.CurrentItemId == itemId) return true;

        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.CurrentItemId == itemId) return true;
        }

        return false;
    }

    private int GetCurrentSpent()
    {
        int total = 0;

        if (lunchSlot != null && lunchSlot.HasItem) total += lunchSlot.CurrentCost;
        if (commuteSlot != null && commuteSlot.HasItem) total += commuteSlot.CurrentCost;

        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.HasItem) total += slot.CurrentCost;
        }

        return total;
    }

    private List<string> GetCurrentSelectedItemIds()
    {
        List<string> ids = new List<string>();

        if (lunchSlot != null && lunchSlot.HasItem) ids.Add(lunchSlot.CurrentItemId);
        if (commuteSlot != null && commuteSlot.HasItem) ids.Add(commuteSlot.CurrentItemId);

        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.HasItem) ids.Add(slot.CurrentItemId);
        }

        return ids;
    }

    private void RefreshBudgetUI()
    {
        if (GameManager.Instance == null) return;

        int allowance = GameManager.Instance.GetTodaySpendableMoney();

        int needsSpent = 0;
        int wantsSpent = 0;

        if (lunchSlot != null && lunchSlot.HasItem)
            needsSpent += lunchSlot.CurrentCost;

        if (commuteSlot != null && commuteSlot.HasItem)
            needsSpent += commuteSlot.CurrentCost;

        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.HasItem)
                wantsSpent += slot.CurrentCost;
        }

        var type = GameManager.Instance.currentBudgetType;

        todaySavings = 0;

        if (type == BudgetType.FiftyThirtyTwenty)
        {
            int needsMax = Mathf.RoundToInt(allowance * 0.5f);
            int wantsMax = Mathf.RoundToInt(allowance * 0.3f);

            int needsRemaining = Mathf.Max(0, needsMax - needsSpent);
            int wantsRemaining = Mathf.Max(0, wantsMax - wantsSpent);

            if (budgetMethodText != null)
                budgetMethodText.text = "Budget Method: 50/30/20";

            if (budgetBudgetText != null)
                budgetBudgetText.text = "Needs Budget: ₱" + needsRemaining;

            todaySavings = needsRemaining + wantsRemaining;
        }
        else if (type == BudgetType.SeventyThirty)
        {
            int sharedMax = Mathf.RoundToInt(allowance * 0.7f);
            int sharedSpent = needsSpent + wantsSpent;
            int sharedRemaining = Mathf.Max(0, sharedMax - sharedSpent);

            if (budgetMethodText != null)
                budgetMethodText.text = "Budget Method: 70/30";

            if (budgetBudgetText != null)
                budgetBudgetText.text = "Needs + Wants Budget: ₱" + sharedRemaining;

            todaySavings = sharedRemaining;
        }
        else
        {
            if (budgetMethodText != null)
                budgetMethodText.text = "Budget Method: None";

            if (budgetBudgetText != null)
                budgetBudgetText.text = "Budget: --";
        }

        if (todaySavings < 0)
            todaySavings = 0;

        int baseSavings = GameManager.Instance.GetCurrentTotalSavings();
        int totalSavings = baseSavings + todaySavings;

        if (totalSavings > savingsGoal)
            totalSavings = savingsGoal;

        if (savingsText != null)
            savingsText.text = "₱" + totalSavings + " / ₱" + savingsGoal;

        if (savingsSlider != null)
        {
            savingsSlider.maxValue = savingsGoal;
            savingsSlider.value = totalSavings;
        }
    }

    private void RefreshAllChoices()
    {
        foreach (var btn in choiceButtons)
        {
            if (btn == null) continue;

            bool placed = IsItemPlaced(btn.ItemId);
            btn.SetPlacedState(placed, isConfirmed);
        }
    }

    private void LockAllSlots(bool locked)
    {
        if (lunchSlot != null) lunchSlot.SetLocked(locked);
        if (commuteSlot != null) commuteSlot.SetLocked(locked);

        foreach (var slot in wantSlots)
        {
            if (slot != null) slot.SetLocked(locked);
        }
    }

    private void SaveTodayAllocation()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.SetDraftAllocation(GetCurrentSelectedItemIds(), GetCurrentSpent());
    }

    private void LoadTodayAllocation()
    {
        if (GameManager.Instance == null) return;

        RefreshBudgetUI();

        if (GameManager.Instance.IsTodayConfirmed())
        {
            isConfirmed = true;

            if (confirmButton != null) confirmButton.interactable = false;
            if (needsButton != null) needsButton.interactable = false;
            if (wantsButton != null) wantsButton.interactable = false;

            LockAllSlots(true);
        }
    }

    public bool HasLunchAndCommuteSelected()
    {
        bool hasLunch = lunchSlot != null && lunchSlot.HasItem;
        bool hasCommute = commuteSlot != null && commuteSlot.HasItem;
        return hasLunch && hasCommute;
    }

    public bool HasRequiredTutorialSelections()
    {
        bool hasLunch = lunchSlot != null && lunchSlot.HasItem;
        bool hasCommute = commuteSlot != null && commuteSlot.HasItem;
        return hasLunch && hasCommute;
    }
}
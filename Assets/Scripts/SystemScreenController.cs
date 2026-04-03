using DG.Tweening;
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

    [Header("Popup Panels")]
    [SerializeField] private RectTransform exitConfirmPanel;
    [SerializeField] private RectTransform allocateWarningPanel;
    [SerializeField] private RectTransform negativeBudgetPanel;
    [SerializeField] private RectTransform confirmAllocationPanel;

    [Header("Exit Popup")]
    [SerializeField] private CanvasGroup exitConfirmPopup;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button exitYesButton;
    [SerializeField] private Button exitNoButton;

    [Header("Warning Popup")]
    [SerializeField] private CanvasGroup allocateWarningPopup;

    [Header("Confirm Allocation Popup")]
    [SerializeField] private CanvasGroup confirmAllocationPopup;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Negative Budget Warning")]
    [SerializeField] private CanvasGroup negativeBudgetPopup;
    [SerializeField] private Button negativeOkButton;

    private bool isBudgetNegative = false;

    [Header("Scripts")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private Day2Intro day2Intro;
    [SerializeField] private Day3Manager day3Manager;
    [SerializeField] private Day4Manager day4Manager;
    [SerializeField] private Day5Manager day5Manager;
    [SerializeField] private Day6Manager day6Manager;
    [SerializeField] private Day7Manager day7Manager;
    [SerializeField] private Day8Manager day8Manager;
    [SerializeField] private Day9Manager day9Manager;
    [SerializeField] private Day10Manager day10Manager;
    [SerializeField] private Day11Manager day11Manager;

    private bool isConfirmed = false;

    private void Start()
    {
        foreach (var btn in choiceButtons)
        {
            if (btn != null) btn.Setup(this);
        }

        SetPopupStateInstant(exitConfirmPopup, false);
        SetPopupStateInstant(allocateWarningPopup, false);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitPressed);

        if (exitYesButton != null)
            exitYesButton.onClick.AddListener(OnExitYesPressed);

        if (exitNoButton != null)
            exitNoButton.onClick.AddListener(OnExitNoPressed);

        SetPopupStateInstant(negativeBudgetPopup, false);

        if (negativeOkButton != null)
        {
            negativeOkButton.onClick.RemoveAllListeners();
            negativeOkButton.onClick.AddListener(CloseNegativeWarning);
        }

        SetPopupStateInstant(confirmAllocationPopup, false);

        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.RemoveAllListeners();
            confirmYesButton.onClick.AddListener(OnConfirmYesPressed);
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.RemoveAllListeners();
            confirmNoButton.onClick.AddListener(OnConfirmNoPressed);
        }

        OpenNeedsTab();
        RefreshAllChoices();
        RefreshBudgetUI();
        LoadTodayAllocation();
    }

    private void CloseNegativeWarning()
    {
        HidePopup(negativeBudgetPopup, negativeBudgetPanel);
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

        if (HasRequiredTutorialSelections() && dialogueController != null)
        {
            dialogueController.ContinueFromButton();
        }
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

        if (HasEmptyRequiredSlots())
        {
            ShowPopup(allocateWarningPopup, allocateWarningPanel);
            return;
        }

        if (isBudgetNegative)
        {
            ShowPopup(negativeBudgetPopup, negativeBudgetPanel);
            return;
        }

        ShowPopup(confirmAllocationPopup, confirmAllocationPanel);
    }

    public void OnConfirmYesPressed()
    {
        HidePopup(confirmAllocationPopup, confirmAllocationPanel);
        ConfirmAllocationNow();
    }

    public void OnConfirmNoPressed()
    {
        HidePopup(confirmAllocationPopup, confirmAllocationPanel);
    }

    private void ConfirmAllocationNow()
    {
        if (isConfirmed) return;

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

        if (type == BudgetType.FiftyThirtyTwenty) // 50/30/20
        {
            int needsMax = Mathf.RoundToInt(allowance * 0.5f);
            int wantsMax = Mathf.RoundToInt(allowance * 0.3f);
            int fixedSavings = allowance - needsMax - wantsMax;

            int needsRemainingRaw = needsMax - needsSpent;
            int wantsRemainingRaw = wantsMax - wantsSpent;

            if (budgetMethodText != null)
                budgetMethodText.text = "Budget Method: 50/30/20";

            if (budgetBudgetText != null)
            {
                string sign = needsRemainingRaw < 0 ? "-" : "";
                int absValue = Mathf.Abs(needsRemainingRaw);
                budgetBudgetText.text = $"Needs Budget: {sign}₱{absValue}";
            }

            isBudgetNegative = (needsRemainingRaw < 0 || wantsRemainingRaw < 0);

            int extraSavings = 0;

            bool hasAnySelection =
                (lunchSlot != null && lunchSlot.HasItem) ||
                (commuteSlot != null && commuteSlot.HasItem);

            foreach (var slot in wantSlots)
            {
                if (slot != null && slot.HasItem)
                {
                    hasAnySelection = true;
                    break;
                }
            }

            if (hasAnySelection)
            {
                extraSavings = Mathf.Max(0, needsRemainingRaw) + Mathf.Max(0, wantsRemainingRaw);
            }

            todaySavings = fixedSavings + extraSavings;
        }

        else if (type == BudgetType.SeventyThirty) // 70/30
        {
            int sharedMax = Mathf.RoundToInt(allowance * 0.7f);
            int fixedSavings = allowance - sharedMax;

            int sharedSpent = needsSpent + wantsSpent;
            int sharedRemainingRaw = sharedMax - sharedSpent;

            if (budgetMethodText != null)
                budgetMethodText.text = "Budget Method: 70/30";

            if (budgetBudgetText != null)
            {
                string sign = sharedRemainingRaw < 0 ? "-" : "";
                int absValue = Mathf.Abs(sharedRemainingRaw);
                budgetBudgetText.text = $"Needs + Wants Budget: {sign}₱{absValue}";
            }

            isBudgetNegative = (sharedRemainingRaw < 0);

            int extraSavings = 0;
            bool hasAnySelection =
                (lunchSlot != null && lunchSlot.HasItem) ||
                (commuteSlot != null && commuteSlot.HasItem);

            foreach (var slot in wantSlots)
            {
                if (slot != null && slot.HasItem)
                {
                    hasAnySelection = true;
                    break;
                }
            }

            if (hasAnySelection)
            {
                extraSavings = Mathf.Max(0, sharedRemainingRaw);
            }

            todaySavings = fixedSavings + extraSavings;
        }
        else
        {
            if (budgetMethodText != null)
                budgetMethodText.text = "Budget Method: None";

            if (budgetBudgetText != null)
                budgetBudgetText.text = "Budget: --";

            isBudgetNegative = false;
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

    public bool HasAnyWantSelected()
    {
        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.HasItem)
                return true;
        }
        return false;
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

    private bool HasEmptyRequiredSlots()
    {
        bool lunchEmpty = (lunchSlot == null || !lunchSlot.HasItem);
        bool commuteEmpty = (commuteSlot == null || !commuteSlot.HasItem);

        return lunchEmpty || commuteEmpty;
    }
    private void SetPopupStateInstant(CanvasGroup popup, bool visible)
    {
        if (popup == null) return;

        popup.DOKill();
        popup.alpha = visible ? 1f : 0f;
        popup.interactable = visible;
        popup.blocksRaycasts = visible;
        popup.gameObject.SetActive(visible);
    }

    private void ShowPopup(CanvasGroup popup, RectTransform panel)
    {
        if (popup == null || panel == null) return;

        popup.DOKill();
        panel.DOKill();

        popup.gameObject.SetActive(true);
        popup.alpha = 0f;
        popup.interactable = true;
        popup.blocksRaycasts = true;

        panel.localScale = Vector3.one * 0.8f;

        popup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic);
        panel.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }

    private void HidePopup(CanvasGroup popup, RectTransform panel)
    {
        if (popup == null || panel == null) return;

        popup.DOKill();
        panel.DOKill();

        popup.interactable = false;
        popup.blocksRaycasts = false;

        popup.DOFade(0f, 0.18f).SetEase(Ease.InCubic);
        panel.DOScale(0.85f, 0.18f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                popup.gameObject.SetActive(false);
            });
    }

    public void OnExitPressed()
    {
        HidePopup(allocateWarningPopup, allocateWarningPanel);
        ShowPopup(exitConfirmPopup, exitConfirmPanel);
    }

    public void OnExitYesPressed()
    {
        bool isConfirmed = GameManager.Instance != null && GameManager.Instance.IsTodayConfirmed();

        if (!isConfirmed)
        {
            HidePopup(exitConfirmPopup, exitConfirmPanel);
            ShowPopup(allocateWarningPopup, allocateWarningPanel);
            return;
        }

        HidePopup(exitConfirmPopup, exitConfirmPanel);
        ProceedExitFlow();
    }

    public void OnExitNoPressed()
    {
        HidePopup(exitConfirmPopup, exitConfirmPanel);
    }

    public void CloseWarningPopup()
    {
        HidePopup(allocateWarningPopup, allocateWarningPanel);
    }

    private void ProceedExitFlow()
    {

        if (day2Intro != null)
        {
            day2Intro.CloseFromSystemScreen();
            return;
        }

        if (day3Manager != null)
        {
            day3Manager.CloseFromSystemScreen();
            return;
        }

        if (day4Manager != null)
        {
            day4Manager.CloseFromSystemScreen();
            return;
        }

        if (day5Manager != null)
        {
            day5Manager.CloseFromSystemScreen();
            return;
        }

        if (day6Manager != null)
        {
            day6Manager.CloseFromSystemScreen();
            return;
        }

        if (day7Manager != null)
        {
            day7Manager.CloseFromSystemScreen();
            return;
        }

        if (day8Manager != null)
        {
            day8Manager.CloseFromSystemScreen();
            return;
        }

        if (day9Manager != null)
        {
            day9Manager.CloseFromSystemScreen();
            return;
        }

        if (day10Manager != null)
        {
            day10Manager.CloseFromSystemScreen();
            return;
        }

        if (day11Manager != null)
        {
            day11Manager.CloseFromSystemScreen();
            return;
        }
    }
}
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SystemScreenController : MonoBehaviour
{
    [Header("Choice Panels")]
    [SerializeField] private GameObject lunchChoicePanel;
    [SerializeField] private GameObject commuteChoicePanel;
    [SerializeField] private GameObject wantsChoicePanel;

    private AllocationSlotUI currentTargetSlot;

    [Header("Tab Buttons")]
    [SerializeField] private Button confirmButton;

    [Header("Need Slots")]
    [SerializeField] private AllocationSlotUI lunchSlot;
    [SerializeField] private AllocationSlotUI commuteSlot;

    [Header("Want Slots")]
    [SerializeField] private AllocationSlotUI[] wantSlots;

    [Header("Budget Info UI")]
    [SerializeField] private Image budgetMethodImage;
    [SerializeField] private Sprite fiftyThirtyTwentySprite;
    [SerializeField] private Sprite seventyThirtySprite;
    [SerializeField] private Sprite noBudgetMethodSprite;
    [SerializeField] private TextMeshProUGUI budgetBudgetText;

    [Header("Budget Tooltip UI")]
    [SerializeField] private CanvasGroup budgetTooltipPopup;
    [SerializeField] private RectTransform budgetTooltipPanel;
    [SerializeField] private TextMeshProUGUI budgetTooltipText;

    [SerializeField] private float hoverFadeDuration = 0.18f;
    [SerializeField] private float hoverPopDuration = 0.22f;
    [SerializeField] private float hoverStartScale = 0.85f;

    private bool isBudgetHovering = false;
    private Tween budgetHoverFadeTween;
    private Tween budgetHoverScaleTween;


    [Header("Savings UI")]
    [SerializeField] private TextMeshProUGUI savingsText;
    [SerializeField] private Image savingsMeterImage;
    [SerializeField] private Sprite savingsEmptySprite;
    [SerializeField] private Sprite savingsAlmostFullSprite;
    [SerializeField] private Sprite savingsHalfFilledSprite;
    [SerializeField] private Sprite savingsSemiFilledSprite;
    [SerializeField] private Sprite savingsFullSprite;
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
    private bool isConfirmed = false;

    public struct ChoiceEvaluation
    {
        public bool isAvailable;
        public int finalCost;
        public float happinessDelta;

        public string baseName;
        public string variantLabel;

        public bool showLowMoodPriceText;
        public string lowMoodPriceLabel;
    }

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
    [SerializeField] private Day12Manager day12Manager;
    [SerializeField] private Day13Manager day13Manager;
    [SerializeField] private Day14Manager day14Manager;
    [SerializeField] private Day15Manager day15Manager;

    [SerializeField] private HappinessMeter happinessMeter;

    const float lowHappinessWantsPriceMultiplier = 1.2f;

    private void Start()
    {
        SetBudgetHoverInstant(false);

        foreach (var btn in choiceButtons)
        {
            if (btn != null) btn.Setup(this);
        }

        if (lunchSlot != null) lunchSlot.SetOwner(this);
        if (commuteSlot != null) commuteSlot.SetOwner(this);

        foreach (var slot in wantSlots)
        {
            if (slot != null) slot.SetOwner(this);
        }

        CloseAllChoicePanels();

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

        Debug.Log("[SystemScreen] Start finished, initial budget/savings computed.", this);
    }

    private void CloseNegativeWarning()
    {
        HidePopup(negativeBudgetPopup, negativeBudgetPanel);
    }

    private void UpdateSavingsMeterSprite(int currentSavings)
    {
        if (savingsMeterImage == null || savingsGoal <= 0) return;

        float ratio = Mathf.Clamp01((float)currentSavings / savingsGoal);

        if (ratio <= 0.10f)
        {
            savingsMeterImage.sprite = savingsEmptySprite;
        }
        else if (ratio <= 0.30f)
        {
            savingsMeterImage.sprite = savingsAlmostFullSprite;
        }
        else if (ratio <= 0.55f)
        {
            savingsMeterImage.sprite = savingsHalfFilledSprite;
        }
        else if (ratio <= 0.85f)
        {
            savingsMeterImage.sprite = savingsSemiFilledSprite;
        }
        else
        {
            savingsMeterImage.sprite = savingsFullSprite;
        }
    }

    public void OpenNeedsTab()
    {
        CloseAllChoicePanels();
    }

    public void OpenWantsTab()
    {
        CloseAllChoicePanels();
    }

    public void OnSlotPressed(AllocationSlotUI slot)
    {
        if (isConfirmed || slot == null) return;

        currentTargetSlot = slot;
        CloseAllChoicePanels();

        switch (slot.Type)
        {
            case AllocationSlotUI.SlotType.Lunch:
                if (lunchChoicePanel != null) lunchChoicePanel.SetActive(true);
                break;

            case AllocationSlotUI.SlotType.Commute:
                if (commuteChoicePanel != null) commuteChoicePanel.SetActive(true);
                break;

            case AllocationSlotUI.SlotType.Want:
                if (wantsChoicePanel != null) wantsChoicePanel.SetActive(true);
                break;
        }
    }

    public void CloseChoicePanels()
    {
        currentTargetSlot = null;
        CloseAllChoicePanels();
    }

    private void CloseAllChoicePanels()
    {
        if (lunchChoicePanel != null) lunchChoicePanel.SetActive(false);
        if (commuteChoicePanel != null) commuteChoicePanel.SetActive(false);
        if (wantsChoicePanel != null) wantsChoicePanel.SetActive(false);
    }

    public void OnChoiceClicked(AllocationItemData item)
    {
        if (isConfirmed || item == null) return;
        if (currentTargetSlot == null) return;

        ChoiceEvaluation eval = EvaluateChoice(item);
        if (!eval.isAvailable) return;

        if (currentTargetSlot.Type == AllocationSlotUI.SlotType.Lunch &&
            item.category != AllocationCategory.NeedLunch)
            return;

        if (currentTargetSlot.Type == AllocationSlotUI.SlotType.Commute &&
            item.category != AllocationCategory.NeedCommute)
            return;

        if (currentTargetSlot.Type == AllocationSlotUI.SlotType.Want &&
            item.category != AllocationCategory.Want)
            return;

        currentTargetSlot.SetItem(item, this);

        if (item.category == AllocationCategory.Want && GameManager.Instance != null)
        {
            GameManager.Instance.RecordWantChoice(item.itemId);
        }

        if (item.itemId == "luto_baon" && GameManager.Instance != null)
        {
            GameManager.Instance.RecordLutoBaonUse();
        }

        CloseAllChoicePanels();
        currentTargetSlot = null;

        RefreshBudgetUI();
        RefreshAllChoices();
        SaveTodayAllocation();

        Debug.Log("Lunch selected: " + (lunchSlot != null && lunchSlot.HasItem), this);
        Debug.Log("Commute selected: " + (commuteSlot != null && commuteSlot.HasItem), this);
        Debug.Log("HasRequiredTutorialSelections: " + HasRequiredTutorialSelections(), this);
        Debug.Log("DialogueController ref exists: " + (dialogueController != null), this);

        if (HasRequiredTutorialSelections() && dialogueController != null)
        {
            dialogueController.ContinueFromAllocationChoices();
        }
    }

    public void OnPlacedItemClicked(string itemId)
    {
        if (isConfirmed) return;

        RemoveItem(itemId);
        RefreshBudgetUI();
        RefreshAllChoices();
        SaveTodayAllocation();

        Debug.Log($"[SystemScreen] Item removed: {itemId}. Recomputed budget/savings.", this);
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

        int happinessDelta = GetCurrentHappinessDelta();

        Debug.Log(
            $"[Allocation Confirm] Starting confirm. " +
            $"Spent = ₱{GetCurrentSpent()}, " +
            $"NeedsSpent = ₱{needsSpent}, " +
            $"WantsSpent = ₱{wantsSpent}, " +
            $"TodaySavings = ₱{todaySavings}, " +
            $"HappinessDelta = {happinessDelta}",
            this
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ConfirmTodayAllocation(
                GetCurrentSpent(),
                GetCurrentSelectedItemIds()
            );

            if (commuteSlot != null && commuteSlot.HasItem && commuteSlot.CurrentItemData != null)
            {
                GameManager.Instance.RecordCommuteChoice(
                    commuteSlot.CurrentItemData.itemId,
                    commuteSlot.CurrentCost
                );

                Debug.Log(
                    $"[Allocation Confirm] Commute recorded: {commuteSlot.CurrentItemData.itemId} | Cost = ₱{commuteSlot.CurrentCost}",
                    this
                );
            }

            GameManager.Instance.SetTodayCategorySpent(needsSpent, wantsSpent);
            GameManager.Instance.SetTodaySaved(todaySavings);
            GameManager.Instance.AddSavings(todaySavings);

            Debug.Log("[Tracker Check] needsSpent = " + needsSpent, this);
            Debug.Log("[Tracker Check] wantsSpent = " + wantsSpent, this);
            Debug.Log("[Tracker Check] totalConfirmedNeedsSpent BEFORE = " + GameManager.Instance.totalConfirmedNeedsSpent, this);
            Debug.Log("[Tracker Check] totalConfirmedWantsSpent BEFORE = " + GameManager.Instance.totalConfirmedWantsSpent, this);
            Debug.Log("[Tracker Check] totalConfirmedAllowance BEFORE = " + GameManager.Instance.totalConfirmedAllowance, this);

            GameManager.Instance.totalConfirmedNeedsSpent += needsSpent;
            GameManager.Instance.totalConfirmedWantsSpent += wantsSpent;
            GameManager.Instance.totalConfirmedAllowance += GameManager.Instance.dailyAllowance;

            Debug.Log("[Tracker Check] totalConfirmedNeedsSpent AFTER = " + GameManager.Instance.totalConfirmedNeedsSpent, this);
            Debug.Log("[Tracker Check] totalConfirmedWantsSpent AFTER = " + GameManager.Instance.totalConfirmedWantsSpent, this);
            Debug.Log("[Tracker Check] totalConfirmedAllowance AFTER = " + GameManager.Instance.totalConfirmedAllowance, this);

            int beforeHappiness = GameManager.Instance.happiness;

            GameManager.Instance.happiness = Mathf.Clamp(
                GameManager.Instance.happiness + happinessDelta,
                0,
                100
            );

            Debug.Log(
                $"[Allocation Confirm] Happiness updated: {beforeHappiness} -> {GameManager.Instance.happiness} (delta: {happinessDelta})",
                this
            );
        }

        if (happinessMeter != null)
        {
            happinessMeter.UpdateVisual();
            Debug.Log("[Allocation Confirm] Happiness meter visual refreshed.", this);
        }

        if (confirmButton != null) confirmButton.interactable = false;

        foreach (var btn in choiceButtons)
        {
            if (btn != null) btn.SetLocked(true);
        }

        LockAllSlots(true);
        CloseAllChoicePanels();

        Debug.Log("[Allocation Confirm] Allocation locked and finalized.", this);
    }

    private void PlaceIntoSlot(AllocationSlotUI slot, AllocationItemData item)
    {
        if (slot == null) return;
        slot.SetItem(item, this);
        RefreshAllChoices();

        Debug.Log($"[SystemScreen] Item placed: {item.itemId} in {slot.Type}.", this);
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

    private int GetCurrentHappinessDelta()
    {
        float total = 0f;

        if (lunchSlot != null && lunchSlot.HasItem && lunchSlot.CurrentItemData != null)
            total += EvaluateChoice(lunchSlot.CurrentItemData).happinessDelta;

        if (commuteSlot != null && commuteSlot.HasItem && commuteSlot.CurrentItemData != null)
            total += EvaluateChoice(commuteSlot.CurrentItemData).happinessDelta;

        int wantsSpent = 0;

        foreach (var slot in wantSlots)
        {
            if (slot != null && slot.HasItem && slot.CurrentItemData != null)
            {
                var eval = EvaluateChoice(slot.CurrentItemData);
                total += eval.happinessDelta;
                wantsSpent += eval.finalCost;
            }
        }

        var gm = GameManager.Instance;
        if (gm != null && wantsSpent > 0)
        {
            int allowance = gm.GetTodaySpendableMoney();
            int wantsBudget = Mathf.RoundToInt(allowance * 0.3f);

            if (wantsSpent <= wantsBudget)
            {
                total += 1f;
            }
            else
            {
                total -= 10f;
            }
        }

        int rounded = Mathf.RoundToInt(total);
        Debug.Log($"[Happiness] Total happiness delta (rounded) = {rounded}", this);
        return rounded;
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

    private void UpdateBudgetMethodSprite(BudgetType type)
    {
        if (budgetMethodImage == null) return;

        switch (type)
        {
            case BudgetType.FiftyThirtyTwenty:
                budgetMethodImage.sprite = fiftyThirtyTwentySprite;
                break;

            case BudgetType.SeventyThirty:
                budgetMethodImage.sprite = seventyThirtySprite;
                break;

            default:
                budgetMethodImage.sprite = noBudgetMethodSprite;
                break;
        }
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
        UpdateBudgetMethodSprite(type);

        todaySavings = 0;

        Debug.Log($"[Budget] Type={type}, allowance=₱{allowance}, needsSpent=₱{needsSpent}, wantsSpent=₱{wantsSpent}", this);

        if (type == BudgetType.FiftyThirtyTwenty)
        {
            int needsMax = Mathf.RoundToInt(allowance * 0.5f);
            int wantsMax = Mathf.RoundToInt(allowance * 0.3f);
            int fixedSavings = allowance - needsMax - wantsMax;

            int needsRemainingRaw = needsMax - needsSpent;
            int wantsRemainingRaw = wantsMax - wantsSpent;

            Debug.Log($"[50/30/20] needsMax=₱{needsMax}, wantsMax=₱{wantsMax}, fixedSavings=₱{fixedSavings}", this);
            Debug.Log($"[50/30/20] needsRemainingRaw=₱{needsRemainingRaw}, wantsRemainingRaw=₱{wantsRemainingRaw}", this);

            // build tooltip breakdown text
            if (budgetTooltipText != null)
            {
                string needsSign = needsRemainingRaw < 0 ? "-" : "";
                string wantsSign = wantsRemainingRaw < 0 ? "-" : "";

                int needsAbs = Mathf.Abs(needsRemainingRaw);
                int wantsAbs = Mathf.Abs(wantsRemainingRaw);

                budgetTooltipText.text =
                    $"Needs budget: {needsSign}₱{needsAbs}\n" +
                    $"Wants budget: {wantsSign}₱{wantsAbs}";
            }

            UpdateBudgetMethodSprite(type);

            // total remaining across needs + wants
            int totalRemainingRaw = needsRemainingRaw + wantsRemainingRaw;

            if (budgetBudgetText != null)
            {
                string sign = totalRemainingRaw < 0 ? "-" : "";
                int absValue = Mathf.Abs(totalRemainingRaw);
                budgetBudgetText.text = $"{sign}₱{absValue}";
            }

            // this must be outside the if-block
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

            Debug.Log($"[50/30/20] hasAnySelection={hasAnySelection}", this);

            if (hasAnySelection)
            {
                extraSavings = Mathf.Max(0, needsRemainingRaw) + Mathf.Max(0, wantsRemainingRaw);
            }

            todaySavings = fixedSavings + extraSavings;

            Debug.Log($"[50/30/20] extraSavings=₱{extraSavings}, todaySavings=₱{todaySavings}, isBudgetNegative={isBudgetNegative}", this);
        }
        else if (type == BudgetType.SeventyThirty)
        {
            int sharedMax = Mathf.RoundToInt(allowance * 0.7f);
            int fixedSavings = allowance - sharedMax;

            int sharedSpent = needsSpent + wantsSpent;
            int sharedRemainingRaw = sharedMax - sharedSpent;

            Debug.Log($"[70/30] sharedMax=₱{sharedMax}, fixedSavings=₱{fixedSavings}", this);
            Debug.Log($"[70/30] sharedSpent=₱{sharedSpent}, sharedRemainingRaw=₱{sharedRemainingRaw}", this);

            UpdateBudgetMethodSprite(type);

            if (budgetBudgetText != null)
            {
                string sign = sharedRemainingRaw < 0 ? "-" : "";
                int absValue = Mathf.Abs(sharedRemainingRaw);
                budgetBudgetText.text = $"{sign}₱{absValue}";
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

            Debug.Log($"[70/30] hasAnySelection={hasAnySelection}", this);

            if (hasAnySelection)
            {
                extraSavings = Mathf.Max(0, sharedRemainingRaw);
            }

            todaySavings = fixedSavings + extraSavings;

            Debug.Log($"[70/30] extraSavings=₱{extraSavings}, todaySavings=₱{todaySavings}, isBudgetNegative={isBudgetNegative}", this);
        }
        else
        {
            UpdateBudgetMethodSprite(type);

            if (budgetBudgetText != null)
                budgetBudgetText.text = "Budget: --";

            isBudgetNegative = false;

            Debug.Log("[Budget] No budget method, todaySavings forced to 0.", this);
        }

        if (todaySavings < 0)
            todaySavings = 0;

        int baseSavings = GameManager.Instance.GetCurrentTotalSavings();
        int totalSavings = baseSavings + todaySavings;

        Debug.Log($"[Savings] baseSavings=₱{baseSavings}, todaySavings=₱{todaySavings}, totalSavings(before cap)=₱{totalSavings}", this);

        if (totalSavings > savingsGoal)
            totalSavings = savingsGoal;

        if (savingsText != null)
            savingsText.text = "₱" + totalSavings + " / ₱" + savingsGoal;

        Debug.Log($"[Savings] totalSavings(after cap)=₱{totalSavings}, savingsGoal=₱{savingsGoal}", this);

        UpdateSavingsMeterSprite(totalSavings);
    }

    public void OnBudgetHoverEnter()
    {
        if (budgetTooltipPopup == null || budgetTooltipPanel == null) return;
        ShowPopup(budgetTooltipPopup, budgetTooltipPanel);
    }

    public void OnBudgetHoverExit()
    {
        if (budgetTooltipPopup == null || budgetTooltipPanel == null) return;
        HidePopup(budgetTooltipPopup, budgetTooltipPanel);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isBudgetHovering = true;
        ShowBudgetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isBudgetHovering = false;
        HideBudgetHover();
    }

    public void ShowBudgetHover()
    {
        if (budgetTooltipPopup == null || budgetTooltipPanel == null)
            return;

        isBudgetHovering = true;

        budgetHoverFadeTween?.Kill();
        budgetHoverScaleTween?.Kill();

        budgetTooltipPopup.gameObject.SetActive(true);
        budgetTooltipPopup.interactable = false;
        budgetTooltipPopup.blocksRaycasts = false;
        budgetTooltipPopup.alpha = 0f;

        budgetTooltipPanel.localScale = Vector3.one * hoverStartScale;

        budgetHoverFadeTween = budgetTooltipPopup
            .DOFade(1f, hoverFadeDuration)
            .SetEase(Ease.OutCubic);

        budgetHoverScaleTween = budgetTooltipPanel
            .DOScale(1f, hoverPopDuration)
            .SetEase(Ease.OutBack);
    }

    public void HideBudgetHover()
    {
        if (budgetTooltipPopup == null || budgetTooltipPanel == null)
            return;

        isBudgetHovering = false;

        budgetHoverFadeTween?.Kill();
        budgetHoverScaleTween?.Kill();

        budgetHoverFadeTween = budgetTooltipPopup
            .DOFade(0f, hoverFadeDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                if (!isBudgetHovering)
                    budgetTooltipPopup.gameObject.SetActive(false);
            });

        budgetHoverScaleTween = budgetTooltipPanel
            .DOScale(hoverStartScale, hoverFadeDuration)
            .SetEase(Ease.InBack);
    }

    private void SetBudgetHoverInstant(bool visible)
    {
        if (budgetTooltipPopup == null || budgetTooltipPanel == null)
            return;

        budgetHoverFadeTween?.Kill();
        budgetHoverScaleTween?.Kill();

        budgetTooltipPopup.gameObject.SetActive(visible);
        budgetTooltipPopup.alpha = visible ? 1f : 0f;
        budgetTooltipPopup.interactable = false;
        budgetTooltipPopup.blocksRaycasts = false;

        budgetTooltipPanel.localScale = visible ? Vector3.one : Vector3.one * hoverStartScale;
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
            btn.SetLocked(isConfirmed);
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

        Debug.Log("[SystemScreen] Draft allocation saved. Spent=₱" + GetCurrentSpent(), this);
    }

    private void LoadTodayAllocation()
    {
        if (GameManager.Instance == null) return;

        RefreshBudgetUI();

        if (GameManager.Instance.IsTodayConfirmed())
        {
            isConfirmed = true;

            if (confirmButton != null) confirmButton.interactable = false;
            LockAllSlots(true);

            Debug.Log("[SystemScreen] Existing confirmed allocation found, locking UI.", this);
        }
        else
        {
            Debug.Log("[SystemScreen] No confirmed allocation yet, UI remains editable.", this);
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
        bool alreadyConfirmed = GameManager.Instance != null && GameManager.Instance.IsTodayConfirmed();

        if (!alreadyConfirmed)
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

    public ChoiceEvaluation EvaluateChoice(AllocationItemData item)
    {
        ChoiceEvaluation result = new ChoiceEvaluation
        {
            isAvailable = true,
            finalCost = item.cost,
            happinessDelta = item.happiness,
            baseName = item.itemName,
            variantLabel = "",
            showLowMoodPriceText = false,
            lowMoodPriceLabel = ""
        };

        var gm = GameManager.Instance;
        int currentHappiness = gm != null ? gm.happiness : 0;

        if (item.category == AllocationCategory.NeedLunch)
        {
            switch (item.itemId)
            {
                case "shoemai_rice":
                    result.baseName = "Shoemai Rice";
                    result.finalCost = 60;
                    result.happinessDelta = 1f;
                    break;

                case "baks_itlog":
                    result.baseName = "Baks Itlog";
                    result.finalCost = 110;
                    result.happinessDelta = 2f;
                    break;

                case "chimken_23":
                    result.baseName = "23Chimken";
                    result.finalCost = 180;
                    result.happinessDelta = 3f;
                    break;

                case "luto_baon":
                    result.baseName = "Luto Baon";
                    result.finalCost = 0;
                    result.happinessDelta = -2f;

                    if (currentHappiness < 50)
                    {
                        result.isAvailable = false;
                        return result;
                    }

                    if (gm != null)
                    {
                        int recentUses = gm.CountUsesWithinDays(gm.lutoBaonUseDays, 5);
                        if (recentUses >= 2)
                        {
                            result.isAvailable = false;
                            return result;
                        }
                    }
                    break;

                case "skip_lunch":
                    result.baseName = "Skip Lunch";
                    result.finalCost = 0;
                    result.happinessDelta = -20f;
                    break;
            }

            return result;
        }

        if (item.category == AllocationCategory.NeedCommute)
        {
            switch (item.itemId)
            {
                case "dalawang_sakay":
                    result.baseName = "Dalawang Sakay";
                    result.finalCost = 50;
                    result.happinessDelta = 0f;
                    break;

                case "joykasit":
                    result.baseName = "Joykasit";
                    result.finalCost = 150;
                    result.happinessDelta = 2f;
                    break;

                case "oober":
                    result.baseName = "OOBER";
                    result.finalCost = 350;
                    result.happinessDelta = 5f;
                    break;

                case "pasabay":
                    result.baseName = "Pasabay";
                    result.finalCost = 0;
                    result.happinessDelta = 0f;

                    if (currentHappiness < 75)
                    {
                        result.isAvailable = false;
                        return result;
                    }
                    break;
            }

            return result;
        }

        if (item.category == AllocationCategory.Want)
        {
            switch (item.itemId)
            {
                case "milk_tea":
                    result.baseName = "Milk tea";
                    result.variantLabel = "Regular";
                    result.finalCost = 150;
                    result.happinessDelta = 3f;

                    if (gm != null && gm.WasWantChosenLastDay("milk_tea"))
                    {
                        result.variantLabel = "Extra Sinkers";
                        result.finalCost = 180;
                        result.happinessDelta = 1f;
                    }
                    break;

                case "coffee":
                    result.baseName = "Coffee";
                    result.variantLabel = "Grande";
                    result.finalCost = 180;
                    result.happinessDelta = 4f;

                    if (gm != null && gm.WasWantChosenLastDay("coffee"))
                    {
                        result.variantLabel = "Venti";
                        result.finalCost = 220;
                        result.happinessDelta = 1.5f;
                    }
                    break;

                case "sorbetes":
                    result.baseName = "Sorbetes";
                    result.variantLabel = "Small Cone";
                    result.finalCost = 20;
                    result.happinessDelta = 1f;

                    if (gm != null && gm.WasWantChosenLastDay("sorbetes"))
                    {
                        result.variantLabel = "Big Cone";
                        result.finalCost = 35;
                        result.happinessDelta = 0f;
                    }
                    break;

                case "want_skip":
                    result.baseName = "Skip";
                    result.variantLabel = "";
                    result.finalCost = 0;
                    result.happinessDelta = -10f;
                    break;
            }

            if (gm != null && gm.happiness < 30 && item.itemId != "want_skip")
            {
                result.finalCost = Mathf.RoundToInt(result.finalCost * lowHappinessWantsPriceMultiplier);
                result.showLowMoodPriceText = true;
                result.lowMoodPriceLabel = "Low Mood Price";
            }

            return result;
        }

        return result;
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

        if (day12Manager != null)
        {
            day12Manager.CloseFromSystemScreen();
            return;
        }

        if (day13Manager != null)
        {
            day13Manager.CloseFromSystemScreen();
            return;
        }

        if (day14Manager != null)
        {
            day14Manager.CloseFromSystemScreen();
            return;
        }

        if (day15Manager != null)
        {
            day15Manager.CloseFromSystemScreen();
            return;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackerScreenController : MonoBehaviour
{
    [Header("Tracker Fills")]
    [SerializeField] private Image needsFillImage;
    [SerializeField] private Image wantsFillImage;
    [SerializeField] private Image savingsFillImage;

    [Header("Tracker Text")]
    [SerializeField] private TextMeshProUGUI needsPercentText;
    [SerializeField] private TextMeshProUGUI wantsPercentText;
    [SerializeField] private TextMeshProUGUI savingsPercentText;

    [Header("Optional Amount Text")]
    [SerializeField] private TextMeshProUGUI needsAmountText;
    [SerializeField] private TextMeshProUGUI wantsAmountText;
    [SerializeField] private TextMeshProUGUI savingsAmountText;

    private void OnEnable()
    {
        RefreshTracker();
    }

    public void RefreshTracker()
    {
        if (GameManager.Instance == null) return;

        int allowance = GameManager.Instance.dailyAllowance;
        BudgetType type = GameManager.Instance.currentBudgetType;

        int needsBudget = 0;
        int wantsBudget = 0;
        int savingsBudget = 0;

        int needsSpent = GameManager.Instance.todayNeedsSpent;
        int wantsSpent = GameManager.Instance.todayWantsSpent;
        int savingsSaved = GameManager.Instance.GetTodaySaved();

        if (type == BudgetType.SeventyThirty)
        {
            needsBudget = Mathf.RoundToInt(allowance * 0.7f);
            wantsBudget = Mathf.RoundToInt(allowance * 0.3f);
            savingsBudget = Mathf.RoundToInt(allowance * 0.3f);
        }
        else if (type == BudgetType.FiftyThirtyTwenty)
        {
            needsBudget = Mathf.RoundToInt(allowance * 0.5f);
            wantsBudget = Mathf.RoundToInt(allowance * 0.3f);
            savingsBudget = Mathf.RoundToInt(allowance * 0.2f);
        }

        float needsRatio = needsBudget > 0 ? Mathf.Clamp01((float)needsSpent / needsBudget) : 0f;
        float wantsRatio = wantsBudget > 0 ? Mathf.Clamp01((float)wantsSpent / wantsBudget) : 0f;
        float savingsRatio = savingsBudget > 0 ? Mathf.Clamp01((float)savingsSaved / savingsBudget) : 0f;

        float needsPercent = needsRatio * 100f;
        float wantsPercent = wantsRatio * 100f;
        float savingsPercent = savingsRatio * 100f;

        if (needsFillImage != null) needsFillImage.fillAmount = needsRatio;
        if (wantsFillImage != null) wantsFillImage.fillAmount = wantsRatio;
        if (savingsFillImage != null) savingsFillImage.fillAmount = savingsRatio;

        if (needsPercentText != null) needsPercentText.text = needsPercent.ToString("F1") + "%";
        if (wantsPercentText != null) wantsPercentText.text = wantsPercent.ToString("F1") + "%";
        if (savingsPercentText != null) savingsPercentText.text = savingsPercent.ToString("F1") + "%";

        if (needsAmountText != null) needsAmountText.text = "₱" + needsSpent + " / ₱" + needsBudget;
        if (wantsAmountText != null) wantsAmountText.text = "₱" + wantsSpent + " / ₱" + wantsBudget;
        if (savingsAmountText != null) savingsAmountText.text = "₱" + savingsSaved + " / ₱" + savingsBudget;
    }
}

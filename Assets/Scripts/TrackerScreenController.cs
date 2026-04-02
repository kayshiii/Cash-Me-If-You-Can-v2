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

        int accumulatedAllowance = Mathf.Max(1, GameManager.Instance.dailyAllowance * GameManager.Instance.currentDay);

        int needsSpent = Mathf.Max(0, GameManager.Instance.todayNeedsSpent);
        int wantsSpent = Mathf.Max(0, GameManager.Instance.todayWantsSpent);
        int savingsSaved = Mathf.Max(0, GameManager.Instance.GetCurrentTotalSavings());

        float needsRatio = Mathf.Clamp01((float)needsSpent / accumulatedAllowance);
        float wantsRatio = Mathf.Clamp01((float)wantsSpent / accumulatedAllowance);
        float savingsRatio = Mathf.Clamp01((float)savingsSaved / accumulatedAllowance);

        float needsPercent = needsRatio * 100f;
        float wantsPercent = wantsRatio * 100f;
        float savingsPercent = savingsRatio * 100f;

        if (needsFillImage != null) needsFillImage.fillAmount = needsRatio;
        if (wantsFillImage != null) wantsFillImage.fillAmount = wantsRatio;
        if (savingsFillImage != null) savingsFillImage.fillAmount = savingsRatio;

        if (needsPercentText != null) needsPercentText.text = needsPercent.ToString("F1") + "%";
        if (wantsPercentText != null) wantsPercentText.text = wantsPercent.ToString("F1") + "%";
        if (savingsPercentText != null) savingsPercentText.text = savingsPercent.ToString("F1") + "%";

        if (needsAmountText != null) needsAmountText.text = "₱" + needsSpent;
        if (wantsAmountText != null) wantsAmountText.text = "₱" + wantsSpent;
        if (savingsAmountText != null) savingsAmountText.text = "₱" + savingsSaved;
    }
}
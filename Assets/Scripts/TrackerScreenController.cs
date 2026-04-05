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

    [Header("Optional Total Allowance Text")]
    [SerializeField] private TextMeshProUGUI totalAllowanceText;

    private void OnEnable()
    {
        RefreshTracker();
    }

    public void RefreshTracker()
    {
        if (GameManager.Instance == null) return;

        int accumulatedAllowance = GameManager.Instance.totalConfirmedAllowance;

        int needsSpent = Mathf.Max(0, GameManager.Instance.totalConfirmedNeedsSpent);
        int wantsSpent = Mathf.Max(0, GameManager.Instance.totalConfirmedWantsSpent);
        int savingsSaved = Mathf.Max(0, GameManager.Instance.GetCurrentTotalSavings());

        float needsRatio = accumulatedAllowance > 0 ? Mathf.Clamp01((float)needsSpent / accumulatedAllowance) : 0f;
        float wantsRatio = accumulatedAllowance > 0 ? Mathf.Clamp01((float)wantsSpent / accumulatedAllowance) : 0f;
        float savingsRatio = accumulatedAllowance > 0 ? Mathf.Clamp01((float)savingsSaved / accumulatedAllowance) : 0f;

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

        if (totalAllowanceText != null)
            totalAllowanceText.text = "Accumulated Allowance: ₱" + accumulatedAllowance;
    }
}
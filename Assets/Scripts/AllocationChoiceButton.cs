using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class AllocationChoiceButton : MonoBehaviour
{
    [SerializeField] private AllocationItemData itemData;
    [SerializeField] private GameObject lockObject;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text variantText;
    [SerializeField] private TMP_Text helperText;

    private SystemScreenController owner;
    private Button button;
    private bool forceLocked = false;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (lockObject != null)
            lockObject.SetActive(false);
    }

    public void Setup(SystemScreenController screen)
    {
        owner = screen;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);

        RefreshState();
    }

    private void OnClicked()
    {
        if (owner == null || itemData == null) return;
        owner.OnChoiceClicked(itemData);
    }

    public void SetLocked(bool locked)
    {
        forceLocked = locked;
        RefreshState();
    }

    public void RefreshState()
    {
        bool unavailable = false;

        SystemScreenController.ChoiceEvaluation eval = default;

        if (owner != null && itemData != null)
        {
            eval = owner.EvaluateChoice(itemData);
            unavailable = !eval.isAvailable;
        }

        bool shouldLock = forceLocked || unavailable;

        if (button != null)
            button.interactable = !shouldLock;

        if (lockObject != null)
            lockObject.SetActive(shouldLock);

        UpdateVisuals(eval, shouldLock);
    }

    private void UpdateVisuals(SystemScreenController.ChoiceEvaluation eval, bool shouldLock)
    {
        if (priceText != null && itemData != null)
            priceText.text = "₱" + eval.finalCost.ToString();

        if (variantText != null)
            variantText.text = string.IsNullOrEmpty(eval.variantLabel) ? "" : eval.variantLabel;

        if (helperText != null)
        {
            if (shouldLock)
                helperText.text = "Unavailable";
            else if (eval.showLowMoodPriceText)
                helperText.text = eval.lowMoodPriceLabel;
            else
                helperText.text = "";
        }
    }
}
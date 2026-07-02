using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AllocationChoiceButton : MonoBehaviour
{
    [SerializeField] private AllocationItemData itemData;
    [SerializeField] private GameObject lockObject;

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

        if (owner != null && itemData != null)
        {
            var eval = owner.EvaluateChoice(itemData);
            unavailable = !eval.isAvailable;
        }

        bool shouldLock = forceLocked || unavailable;

        if (button != null)
            button.interactable = !shouldLock;

        if (lockObject != null)
        {
            lockObject.SetActive(shouldLock);

            RectTransform rt = lockObject.GetComponent<RectTransform>();
            Debug.Log(
                $"[ChoiceLock] {itemData?.itemId} " +
                $"shouldLock={shouldLock}, " +
                $"activeSelf={lockObject.activeSelf}, " +
                $"activeInHierarchy={lockObject.activeInHierarchy}, " +
                $"pos={(rt != null ? rt.anchoredPosition.ToString() : "no RectTransform")}, " +
                $"size={(rt != null ? rt.sizeDelta.ToString() : "no RectTransform")}",
                this
            );
        }
        else
        {
            Debug.Log($"[ChoiceLock] {itemData?.itemId} lockObject is NULL", this);
        }
    }
}
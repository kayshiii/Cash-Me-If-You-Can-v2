using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AllocationChoiceButton : MonoBehaviour
{
    [SerializeField] private AllocationItemData itemData;

    private SystemScreenController owner;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Setup(SystemScreenController screen)
    {
        owner = screen;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        if (owner == null || itemData == null) return;
        owner.OnChoiceClicked(itemData);
    }

    public void SetLocked(bool locked)
    {
        if (button != null)
            button.interactable = !locked;
    }
}
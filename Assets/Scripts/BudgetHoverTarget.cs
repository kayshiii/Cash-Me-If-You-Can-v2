using UnityEngine;
using UnityEngine.EventSystems;

public class BudgetHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SystemScreenController systemScreen;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (systemScreen != null)
            systemScreen.ShowBudgetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (systemScreen != null)
            systemScreen.HideBudgetHover();
    }
}
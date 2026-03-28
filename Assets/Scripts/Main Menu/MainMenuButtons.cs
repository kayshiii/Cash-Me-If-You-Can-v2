using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.1f;
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float clickDuration = 0.08f;

    private RectTransform rect;
    private Vector3 originalScale;
    private Tween currentTween;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayScale(hoverScale, hoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PlayScale(1f, hoverDuration);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // quick pop: shrink then return to normal
        if (currentTween != null) currentTween.Kill();
        rect.localScale = originalScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(clickScale, clickDuration).SetEase(Ease.OutQuad));
        seq.Append(rect.DOScale(1f, clickDuration).SetEase(Ease.OutBack));
        currentTween = seq;
    }

    private void PlayScale(float target, float duration)
    {
        if (currentTween != null) currentTween.Kill();
        currentTween = rect.DOScale(originalScale * target, duration).SetEase(Ease.OutQuad);
    }
}

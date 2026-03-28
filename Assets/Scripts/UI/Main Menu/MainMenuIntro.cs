using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuIntro : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private float titleDuration = 0.6f;
    [SerializeField] private float titleOvershoot = 1.1f; // scale overshoot

    [Header("Menu Panel (buttons parent)")]
    [SerializeField] private RectTransform menuPanelRect;
    [SerializeField] private float menuDuration = 0.5f;
    [SerializeField] private float delayAfterTitle = 0.2f;

    private Vector3 titleOriginalPos;
    private Vector3 menuOriginalPos;

    private void Awake()
    {
        // Cache original positions
        if (titleRect != null)
            titleOriginalPos = titleRect.anchoredPosition;
        if (menuPanelRect != null)
            menuOriginalPos = menuPanelRect.anchoredPosition;

        // Set starting states
        if (titleRect != null)
        {
            titleRect.anchoredPosition = new Vector2(titleOriginalPos.x, titleOriginalPos.y + 300f); // above screen
            titleRect.localScale = Vector3.zero;
        }

        if (menuPanelRect != null)
        {
            menuPanelRect.anchoredPosition = new Vector2(menuOriginalPos.x, menuOriginalPos.y - 300f); // below screen
            menuPanelRect.localScale = Vector3.zero;
        }
    }

    private void Start()
    {
        PlayIntro();
    }

    public void PlayIntro()
    {
        Sequence seq = DOTween.Sequence();

        // Title comes in: move + scale
        if (titleRect != null)
        {
            seq.Append(
                titleRect.DOAnchorPos(titleOriginalPos, titleDuration).SetEase(Ease.OutCubic)
            );
            seq.Join(
                titleRect.DOScale(titleOvershoot, titleDuration * 0.7f).SetEase(Ease.OutBack)
            );
            seq.Append(
                titleRect.DOScale(1f, 0.15f)
            );
        }

        // Then menu panel/buttons pop in
        if (menuPanelRect != null)
        {
            seq.AppendInterval(delayAfterTitle);
            seq.Append(
                menuPanelRect.DOAnchorPos(menuOriginalPos, menuDuration).SetEase(Ease.OutCubic)
            );
            seq.Join(
                menuPanelRect.DOScale(1f, menuDuration).SetEase(Ease.OutBack)
            );
        }
    }
}

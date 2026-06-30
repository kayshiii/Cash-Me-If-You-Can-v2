using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HappinessMeter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Meter Pointer")]
    [SerializeField] private RectTransform pointer;   // arrow icon
    [SerializeField] private float minX = -180f;      // position at 0%
    [SerializeField] private float maxX = 190f;       // position at 100%
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    [Header("Hover UI")]
    [SerializeField] private CanvasGroup hoverInfoGroup;
    [SerializeField] private RectTransform hoverInfoRect;
    [SerializeField] private TextMeshProUGUI percentageText;

    [Header("Hover Animation")]
    [SerializeField] private float hoverFadeDuration = 0.18f;
    [SerializeField] private float hoverPopDuration = 0.22f;
    [SerializeField] private float hoverStartScale = 0.85f;

    private int lastHappiness = int.MinValue;
    private bool isHovering = false;
    private Tween hoverFadeTween;
    private Tween hoverScaleTween;
    private Tween pointerTween;

    private void Awake()
    {
        // Pointer is assigned via Inspector; no Image sprite needed anymore
        SetHoverStateInstant(false);
    }

    private void Start()
    {
        UpdateVisual();
    }

    private void OnEnable()
    {
        UpdateVisual();
        SetHoverStateInstant(false);
    }

    private void OnDisable()
    {
        hoverFadeTween?.Kill();
        hoverScaleTween?.Kill();
        pointerTween?.Kill();
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        int current = GameManager.Instance.GetHappinessPercent();

        if (current != lastHappiness)
        {
            UpdateVisual();
        }
    }

    public void UpdateVisual()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[HappinessMeter] GameManager.Instance is null.", this);
            return;
        }

        if (pointer == null)
        {
            Debug.LogWarning("[HappinessMeter] pointer is not assigned.", this);
            return;
        }

        int happiness = GameManager.Instance.GetHappinessPercent();
        lastHappiness = happiness;

        // Clamp 0–100, convert to 0–1
        float t = Mathf.Clamp01(happiness / 100f);

        // Lerp between minX and maxX
        float targetX = Mathf.Lerp(minX, maxX, t);

        // Animate pointer
        pointerTween?.Kill();
        pointerTween = pointer.DOAnchorPosX(targetX, moveDuration)
                              .SetEase(moveEase);

        // Update hover percentage text
        if (percentageText != null)
            percentageText.text = happiness + "%";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        ShowHoverInfo();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        HideHoverInfo();
    }

    private void ShowHoverInfo()
    {
        if (hoverInfoGroup == null || hoverInfoRect == null)
            return;

        hoverFadeTween?.Kill();
        hoverScaleTween?.Kill();

        if (percentageText != null && GameManager.Instance != null)
            percentageText.text = GameManager.Instance.GetHappinessPercent() + "%";

        hoverInfoGroup.gameObject.SetActive(true);
        hoverInfoGroup.interactable = false;
        hoverInfoGroup.blocksRaycasts = false;
        hoverInfoGroup.alpha = 0f;

        hoverInfoRect.localScale = Vector3.one * hoverStartScale;

        hoverFadeTween = hoverInfoGroup
            .DOFade(1f, hoverFadeDuration)
            .SetEase(Ease.OutCubic);

        hoverScaleTween = hoverInfoRect
            .DOScale(1f, hoverPopDuration)
            .SetEase(Ease.OutBack);
    }

    private void HideHoverInfo()
    {
        if (hoverInfoGroup == null || hoverInfoRect == null)
            return;

        hoverFadeTween?.Kill();
        hoverScaleTween?.Kill();

        hoverFadeTween = hoverInfoGroup
            .DOFade(0f, hoverFadeDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                if (!isHovering)
                    hoverInfoGroup.gameObject.SetActive(false);
            });

        hoverScaleTween = hoverInfoRect
            .DOScale(hoverStartScale, hoverFadeDuration)
            .SetEase(Ease.InBack);
    }

    private void SetHoverStateInstant(bool visible)
    {
        if (hoverInfoGroup == null || hoverInfoRect == null)
            return;

        hoverFadeTween?.Kill();
        hoverScaleTween?.Kill();

        hoverInfoGroup.gameObject.SetActive(visible);
        hoverInfoGroup.alpha = visible ? 1f : 0f;
        hoverInfoGroup.interactable = false;
        hoverInfoGroup.blocksRaycasts = false;

        hoverInfoRect.localScale = visible ? Vector3.one : Vector3.one * hoverStartScale;
    }
}
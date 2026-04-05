using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HappinessMeter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Meter Image")]
    [SerializeField] private Image meterImage;
    [SerializeField] private Sprite sadSprite;
    [SerializeField] private Sprite mehSprite;
    [SerializeField] private Sprite happySprite;

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

    private void Awake()
    {
        if (meterImage == null)
            meterImage = GetComponent<Image>();

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
        if (meterImage == null)
        {
            Debug.LogWarning("[HappinessMeter] meterImage is not assigned.", this);
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[HappinessMeter] GameManager.Instance is null.", this);
            return;
        }

        int happiness = GameManager.Instance.GetHappinessPercent();
        lastHappiness = happiness;

        if (happiness <= 30)
        {
            meterImage.sprite = sadSprite;
        }
        else if (happiness <= 70)
        {
            meterImage.sprite = mehSprite;
        }
        else
        {
            meterImage.sprite = happySprite;
        }

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
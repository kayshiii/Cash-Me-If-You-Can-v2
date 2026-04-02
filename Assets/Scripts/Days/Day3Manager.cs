using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Day3Manager : MonoBehaviour
{
    [Header("Intro BG / Title")]
    [SerializeField] private CanvasGroup introBg;
    [SerializeField] private RectTransform dayTitle;
    [SerializeField] private float bgFadeInDuration = 0.6f;
    [SerializeField] private float titlePopDuration = 0.4f;
    [SerializeField] private float titleDelayAfterBg = 0.2f;
    [SerializeField] private float titleFadeOutDuration = 0.3f;
    [SerializeField] private float bgFadeOutDuration = 0.5f;
    [SerializeField] private CanvasGroup campusBg;
    [SerializeField] private float campusFadeInDuration = 0.6f;
    [SerializeField] private float delayBeforeCampus = 0.1f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData midDialogue;
    [SerializeField] private DialogueData finalDialoguePayNow;
    [SerializeField] private DialogueData finalDialoguePayLater;

    [Header("Phone UI")]
    [SerializeField] private CanvasGroup phoneGroup;
    [SerializeField] private RectTransform phoneRect;
    [SerializeField] private float phoneEaseDuration = 0.5f;
    [SerializeField] private float phoneEaseOffsetY = -300f;

    [Header("Notification UI")]
    [SerializeField] private CanvasGroup notificationGroup;
    [SerializeField] private float notificationFadeDuration = 0.3f;
    [SerializeField] private float notificationHoldDuration = 1.5f;

    [Header("Decision Popup")]
    [SerializeField] private CanvasGroup decisionPopup;
    [SerializeField] private RectTransform decisionPanel;
    [SerializeField] private Button payNowButton;
    [SerializeField] private Button payLaterButton;

    [Header("Debt Settings")]
    [SerializeField] private int debtAmount = 500;

    [Header("Ending")]
    [SerializeField] private CanvasGroup endReportLevelGroup;
    [SerializeField] private float endReportFadeDuration = 0.4f;
    [SerializeField] private EODReport endOfDayReportUI;

    [Header("Next Day Button")]
    [SerializeField] private Button nextDayButton;
    [SerializeField] private float nextDayButtonDelay = 3f;
    [SerializeField] private string nextSceneName = "Day 4";

    private Vector3 titleOriginalScale;
    private Vector2 phoneOriginalPos;
    private bool chosePayNow = false;
    private bool decisionMade = false;

    private enum Phase
    {
        IntroAnim,
        IntroDialogue,
        PhoneReveal,
        Notification,
        MidDialogue,
        PhoneReturn,
        DecisionPopup,
        FinalDialogue,
        EndOfDay
    }

    private Phase currentPhase = Phase.IntroAnim;

    private void Awake()
    {
        if (introBg != null)
            introBg.alpha = 0f;

        if (dayTitle != null)
        {
            titleOriginalScale = dayTitle.localScale;
            dayTitle.localScale = Vector3.zero;
        }

        if (campusBg != null)
            campusBg.alpha = 0f;

        if (phoneRect != null)
        {
            phoneOriginalPos = phoneRect.anchoredPosition;
            phoneRect.anchoredPosition = phoneOriginalPos + new Vector2(0f, phoneEaseOffsetY);
        }

        if (phoneGroup != null)
        {
            phoneGroup.alpha = 0f;
            phoneGroup.gameObject.SetActive(false);
        }

        if (notificationGroup != null)
        {
            notificationGroup.alpha = 0f;
            notificationGroup.gameObject.SetActive(false);
        }

        SetPopupStateInstant(decisionPopup, false);

        if (endReportLevelGroup != null)
        {
            endReportLevelGroup.alpha = 0f;
            endReportLevelGroup.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (payNowButton != null)
        {
            payNowButton.onClick.RemoveAllListeners();
            payNowButton.onClick.AddListener(OnPayNowPressed);
        }

        if (payLaterButton != null)
        {
            payLaterButton.onClick.RemoveAllListeners();
            payLaterButton.onClick.AddListener(OnPayLaterPressed);
        }

        PlayIntroAnim();
    }

    private void PlayIntroAnim()
    {
        currentPhase = Phase.IntroAnim;

        Sequence seq = DOTween.Sequence();

        if (introBg != null)
            seq.Append(introBg.DOFade(1f, bgFadeInDuration).SetEase(Ease.OutCubic));

        if (dayTitle != null)
        {
            seq.AppendInterval(titleDelayAfterBg);
            dayTitle.localScale = Vector3.zero;
            seq.Append(dayTitle.DOScale(1f, titlePopDuration).SetEase(Ease.OutBack));
        }

        seq.AppendInterval(0.3f);

        if (dayTitle != null)
            seq.Append(dayTitle.DOScale(0f, titleFadeOutDuration).SetEase(Ease.InBack));

        if (introBg != null)
            seq.Join(introBg.DOFade(0f, bgFadeOutDuration).SetEase(Ease.InCubic));

        if (campusBg != null)
        {
            seq.AppendInterval(delayBeforeCampus);
            seq.Append(campusBg.DOFade(1f, campusFadeInDuration).SetEase(Ease.OutCubic));
        }

        seq.OnComplete(StartIntroDialogue);
    }

    private void StartIntroDialogue()
    {
        currentPhase = Phase.IntroDialogue;

        if (dialogueController != null && introDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(introDialogue, OnIntroDialogueFinished);
        }
    }

    private void OnIntroDialogueFinished()
    {
        ShowPhoneFirstTime();
    }

    private void ShowPhoneFirstTime()
    {
        currentPhase = Phase.PhoneReveal;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        AnimatePhoneIn(ShowNotification);
    }

    private void ShowNotification()
    {
        currentPhase = Phase.Notification;

        if (notificationGroup == null)
        {
            HidePhoneAndStartMidDialogue();
            return;
        }

        notificationGroup.DOKill();
        notificationGroup.gameObject.SetActive(true);
        notificationGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(notificationGroup.DOFade(1f, notificationFadeDuration).SetEase(Ease.OutCubic));
        seq.AppendInterval(notificationHoldDuration);
        seq.OnComplete(HidePhoneAndStartMidDialogue);
    }

    private void HidePhoneAndStartMidDialogue()
    {
        if (notificationGroup != null)
        {
            notificationGroup.DOKill();
        }

        Sequence seq = DOTween.Sequence();

        if (notificationGroup != null && notificationGroup.gameObject.activeSelf)
        {
            seq.Join(notificationGroup.DOFade(0f, notificationFadeDuration).SetEase(Ease.InCubic));
        }

        if (phoneGroup != null)
        {
            phoneGroup.DOKill();
            seq.Join(phoneGroup.DOFade(0f, phoneEaseDuration).SetEase(Ease.InCubic));
        }

        if (phoneRect != null)
        {
            phoneRect.DOKill();
            seq.Join(phoneRect.DOAnchorPos(phoneOriginalPos + new Vector2(0f, phoneEaseOffsetY), phoneEaseDuration).SetEase(Ease.InBack));
        }

        seq.OnComplete(() =>
        {
            if (notificationGroup != null)
            {
                notificationGroup.gameObject.SetActive(false);
            }

            if (phoneGroup != null)
            {
                phoneGroup.gameObject.SetActive(false);
            }

            StartMidDialogue();
        });
    }

    private void StartMidDialogue()
    {
        currentPhase = Phase.MidDialogue;

        if (dialogueController != null && midDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(midDialogue, OnMidDialogueFinished);
        }
    }

    private void OnMidDialogueFinished()
    {
        ShowPhoneForDecision();
    }

    private void ShowPhoneForDecision()
    {
        currentPhase = Phase.PhoneReturn;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        AnimatePhoneIn(() =>
        {
            ShowDecisionPopup();
        });
    }

    private void AnimatePhoneIn(TweenCallback onComplete = null)
    {
        if (phoneGroup == null || phoneRect == null)
        {
            onComplete?.Invoke();
            return;
        }

        phoneGroup.DOKill();
        phoneRect.DOKill();

        phoneGroup.gameObject.SetActive(true);
        phoneGroup.alpha = 0f;
        phoneRect.anchoredPosition = phoneOriginalPos + new Vector2(0f, phoneEaseOffsetY);

        Sequence seq = DOTween.Sequence();
        seq.Append(phoneGroup.DOFade(1f, phoneEaseDuration).SetEase(Ease.OutCubic));
        seq.Join(phoneRect.DOAnchorPos(phoneOriginalPos, phoneEaseDuration).SetEase(Ease.OutBack));
        seq.OnComplete(() => onComplete?.Invoke());
    }

    private void ShowDecisionPopup()
    {
        currentPhase = Phase.DecisionPopup;
        decisionMade = false;

        ShowPopup(decisionPopup, decisionPanel);
    }

    public void OnPayNowPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        chosePayNow = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDay3Decision(true);
        }

        HidePopup(decisionPopup, decisionPanel, HidePhoneThenStartFinalDialogue);
    }

    public void OnPayLaterPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        chosePayNow = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDay3Decision(false);
        }

        HidePopup(decisionPopup, decisionPanel, HidePhoneThenStartFinalDialogue);
    }

    private void HidePhoneThenStartFinalDialogue()
    {
        Sequence seq = DOTween.Sequence();

        if (phoneGroup != null)
        {
            phoneGroup.DOKill();
            seq.Join(phoneGroup.DOFade(0f, phoneEaseDuration).SetEase(Ease.InCubic));
        }

        if (phoneRect != null)
        {
            phoneRect.DOKill();
            seq.Join(phoneRect.DOAnchorPos(phoneOriginalPos + new Vector2(0f, phoneEaseOffsetY), phoneEaseDuration).SetEase(Ease.InBack));
        }

        seq.OnComplete(() =>
        {
            if (phoneGroup != null)
                phoneGroup.gameObject.SetActive(false);

            StartFinalDialogue();
        });
    }

    private void StartFinalDialogue()
    {
        currentPhase = Phase.FinalDialogue;

        if (dialogueController == null) return;

        DialogueData chosenDialogue = chosePayNow ? finalDialoguePayNow : finalDialoguePayLater;

        if (chosenDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(chosenDialogue, OnFinalDialogueFinished);
        }
        else
        {
            OnFinalDialogueFinished();
        }
    }

    private void OnFinalDialogueFinished()
    {
        if (GameManager.Instance != null && chosePayNow)
        {
            GameManager.Instance.ApplyDay3DebtNow(debtAmount);
        }

        if (endOfDayReportUI != null)
        {
            if (chosePayNow)
            {
                endOfDayReportUI.SetSpecialMessage(
                    "You paid the ₱500 debt today. Your savings took a hit, but at least that problem is finally settled."
                );
            }
            else
            {
                endOfDayReportUI.SetSpecialMessage(
                    "You decided to delay the ₱500 debt for now. You protected your savings today, but that unpaid problem is still hanging over you."
                );
            }
        }

        ShowEOD();
    }

    private void ShowEOD()
    {
        currentPhase = Phase.EndOfDay;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        if (endReportLevelGroup != null)
        {
            endReportLevelGroup.gameObject.SetActive(true);
            endReportLevelGroup.alpha = 0f;
            endReportLevelGroup.DOFade(1f, endReportFadeDuration).SetEase(Ease.OutCubic);
        }

        if (endOfDayReportUI != null)
        {
            endOfDayReportUI.gameObject.SetActive(true);
        }
    }

    private void SetPopupStateInstant(CanvasGroup popup, bool visible)
    {
        if (popup == null) return;

        popup.DOKill();
        popup.alpha = visible ? 1f : 0f;
        popup.interactable = visible;
        popup.blocksRaycasts = visible;
        popup.gameObject.SetActive(visible);
    }

    private void ShowPopup(CanvasGroup popup, RectTransform panel)
    {
        if (popup == null || panel == null) return;

        popup.DOKill();
        panel.DOKill();

        popup.gameObject.SetActive(true);
        popup.alpha = 0f;
        popup.interactable = true;
        popup.blocksRaycasts = true;

        panel.localScale = Vector3.one * 0.8f;

        popup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic);
        panel.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }

    private void HidePopup(CanvasGroup popup, RectTransform panel, TweenCallback onComplete = null)
    {
        if (popup == null || panel == null)
        {
            onComplete?.Invoke();
            return;
        }

        popup.DOKill();
        panel.DOKill();

        popup.interactable = false;
        popup.blocksRaycasts = false;

        popup.DOFade(0f, 0.18f).SetEase(Ease.InCubic);
        panel.DOScale(0.85f, 0.18f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                popup.gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    public void LoadNextDayScene()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AdvanceDay();

        SceneManager.LoadScene(nextSceneName);
    }
}
using System.Collections;
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

    [Header("Backgrounds")]
    [SerializeField] private CanvasGroup homeBg;
    [SerializeField] private float homeFadeInDuration = 0.6f;
    [SerializeField] private float delayBeforeHome = 0.1f;
    [SerializeField] private CanvasGroup schoolBg;
    [SerializeField] private float schoolFadeDuration = 0.5f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData postBudgetDialogue;
    [SerializeField] private DialogueData campusIntroDialogue;
    [SerializeField] private DialogueData debtPromptDialogue;
    [SerializeField] private DialogueData finalDialoguePayNow;
    [SerializeField] private DialogueData finalDialoguePayLater;
    [SerializeField] private DialogueData endingDialogue;

    [Header("Phone UI")]
    [SerializeField] private CanvasGroup phoneGroup;
    [SerializeField] private RectTransform phoneRect;
    [SerializeField] private float phoneEaseDuration = 0.5f;
    [SerializeField] private float phoneEaseOffsetY = -300f;

    [Header("App Screens")]
    [SerializeField] private CanvasGroup startScreenGroup;
    [SerializeField] private CanvasGroup systemScreenGroup;
    [SerializeField] private CanvasGroup trackScreenGroup;

    [Header("Gameplay Buttons")]
    [SerializeField] private Button logExpenseButton;
    [SerializeField] private Button trackTabButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button finishDayButton;

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
    private bool hasOpenedPhoneForGameplay = false;
    private bool isOpeningPhone = false;
    private bool chosePayNow = false;
    private bool decisionMade = false;

    private enum Phase
    {
        IntroAnim,
        HomeIntroDialogue,
        BudgetGameplay,
        PostBudgetDialogue,
        SwitchToCampus,
        CampusDialogue,
        NotificationPhone,
        DebtPromptDialogue,
        DecisionPopup,
        FinalChoiceDialogue,
        EndingDialogue,
        EndOfDay,
        Done
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

        if (homeBg != null)
            homeBg.alpha = 0f;

        if (schoolBg != null)
            schoolBg.alpha = 0f;

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

        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);

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

        if (nextDayButton != null)
            nextDayButton.gameObject.SetActive(false);

        SetButtonState(logExpenseButton, false);
        SetButtonState(trackTabButton, false);
        SetButtonState(backButton, false);
        SetButtonState(finishDayButton, false);
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

        if (finishDayButton != null)
        {
            finishDayButton.onClick.RemoveAllListeners();
            finishDayButton.onClick.AddListener(FinishBudgetingPhase);
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

        if (homeBg != null)
        {
            seq.AppendInterval(delayBeforeHome);
            seq.Append(homeBg.DOFade(1f, homeFadeInDuration).SetEase(Ease.OutCubic));
        }

        seq.OnComplete(StartHomeIntroDialogue);
    }

    private void StartHomeIntroDialogue()
    {
        currentPhase = Phase.HomeIntroDialogue;

        if (dialogueController != null && introDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(introDialogue, OnHomeIntroDialogueFinished);
        }
    }

    private void OnHomeIntroDialogueFinished()
    {
        OpenPhoneForGameplay();
    }

    private void OpenPhoneForGameplay()
    {
        if (hasOpenedPhoneForGameplay || isOpeningPhone) return;

        currentPhase = Phase.BudgetGameplay;
        isOpeningPhone = true;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        AnimatePhoneIn(() =>
        {
            hasOpenedPhoneForGameplay = true;
            isOpeningPhone = false;
            ShowStartScreen();

            SetButtonState(logExpenseButton, true);
            SetButtonState(trackTabButton, true);
            SetButtonState(backButton, true);
            SetButtonState(finishDayButton, true);
        });
    }

    public void ShowStartScreen()
    {
        SetCanvasGroupState(startScreenGroup, true);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);
    }

    public void ShowSystemScreen()
    {
        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, true);
        SetCanvasGroupState(trackScreenGroup, false);
    }

    public void ShowTrackScreen()
    {
        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, true);
    }

    public void FinishBudgetingPhase()
    {
        if (currentPhase != Phase.BudgetGameplay) return;

        SetButtonState(logExpenseButton, false);
        SetButtonState(trackTabButton, false);
        SetButtonState(backButton, false);
        SetButtonState(finishDayButton, false);

        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);

        HidePhone(() =>
        {
            StartPostBudgetDialogue();
        });
    }

    private void StartPostBudgetDialogue()
    {
        currentPhase = Phase.PostBudgetDialogue;

        if (dialogueController != null && postBudgetDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(postBudgetDialogue, OnPostBudgetDialogueFinished);
        }
        else
        {
            OnPostBudgetDialogueFinished();
        }
    }

    private void OnPostBudgetDialogueFinished()
    {
        SwitchToCampus();
    }
    public void CloseFromSystemScreen()
    {
        if (currentPhase != Phase.BudgetGameplay) return;

        Debug.Log("[Day3] CloseFromSystemScreen called. Proceeding to post-budget flow.", this);
        FinishBudgetingPhase();
    }
    private void SwitchToCampus()
    {
        currentPhase = Phase.SwitchToCampus;

        Sequence seq = DOTween.Sequence();

        if (homeBg != null)
            seq.Append(homeBg.DOFade(0f, schoolFadeDuration).SetEase(Ease.InOutCubic));

        if (schoolBg != null)
            seq.Join(schoolBg.DOFade(1f, schoolFadeDuration).SetEase(Ease.InOutCubic));

        seq.OnComplete(StartCampusDialogue);
    }

    private void StartCampusDialogue()
    {
        currentPhase = Phase.CampusDialogue;

        if (dialogueController != null && campusIntroDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(campusIntroDialogue, OnCampusDialogueFinished);
        }
        else
        {
            OnCampusDialogueFinished();
        }
    }

    private void OnCampusDialogueFinished()
    {
        ShowPhoneForNotification();
    }

    private void ShowPhoneForNotification()
    {
        currentPhase = Phase.NotificationPhone;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        AnimatePhoneIn(ShowNotification);
    }

    private void ShowNotification()
    {
        if (notificationGroup == null)
        {
            HidePhone(StartDebtPromptDialogue);
            return;
        }

        notificationGroup.DOKill();
        notificationGroup.gameObject.SetActive(true);
        notificationGroup.alpha = 0f;

        Sequence seq = DOTween.Sequence();
        seq.Append(notificationGroup.DOFade(1f, notificationFadeDuration).SetEase(Ease.OutCubic));
        seq.AppendInterval(notificationHoldDuration);
        seq.OnComplete(() =>
        {
            HidePhoneAndNotification(StartDebtPromptDialogue);
        });
    }

    private void StartDebtPromptDialogue()
    {
        currentPhase = Phase.DebtPromptDialogue;

        if (dialogueController != null && debtPromptDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(debtPromptDialogue, OnDebtPromptDialogueFinished);
        }
        else
        {
            OnDebtPromptDialogueFinished();
        }
    }

    private void OnDebtPromptDialogueFinished()
    {
        ShowPhoneForDecision();
    }

    private void ShowPhoneForDecision()
    {
        currentPhase = Phase.DecisionPopup;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        AnimatePhoneIn(() =>
        {
            ShowPopup(decisionPopup, decisionPanel);
            decisionMade = false;
        });
    }

    public void OnPayNowPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        chosePayNow = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDay3Decision(true);

            // Apply debt immediately from current savings
            GameManager.Instance.ApplyDay3DebtNow(debtAmount);

            // Clear any delayed debt in case this was previously set
            GameManager.Instance.hasPendingFinalDebt = false;
            GameManager.Instance.pendingFinalDebtAmount = 0;
        }

        HidePopup(decisionPopup, decisionPanel, () =>
        {
            HidePhone(StartFinalChoiceDialogue);
        });
    }

    public void OnPayLaterPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        chosePayNow = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetDay3Decision(false);

            // Store debt to be deducted from final savings later
            GameManager.Instance.hasPendingFinalDebt = true;
            GameManager.Instance.pendingFinalDebtAmount = debtAmount;
        }

        HidePopup(decisionPopup, decisionPanel, () =>
        {
            HidePhone(StartFinalChoiceDialogue);
        });
    }

    private void StartFinalChoiceDialogue()
    {
        currentPhase = Phase.FinalChoiceDialogue;

        DialogueData chosenDialogue = chosePayNow ? finalDialoguePayNow : finalDialoguePayLater;

        if (dialogueController != null && chosenDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(chosenDialogue, OnFinalChoiceDialogueFinished);
        }
        else
        {
            OnFinalChoiceDialogueFinished();
        }
    }

    private void OnFinalChoiceDialogueFinished()
    {
        StartEndingDialogue();
    }

    private void StartEndingDialogue()
    {
        currentPhase = Phase.EndingDialogue;

        if (dialogueController != null && endingDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(endingDialogue, OnEndingDialogueFinished);
        }
        else
        {
            OnEndingDialogueFinished();
        }
    }

    private void OnEndingDialogueFinished()
    {
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
            endOfDayReportUI.RefreshUI();
        }

        StartCoroutine(ShowNextDayButtonAfterDelay());
    }

    private IEnumerator ShowNextDayButtonAfterDelay()
    {
        if (nextDayButton != null)
            nextDayButton.gameObject.SetActive(false);

        yield return new WaitForSeconds(nextDayButtonDelay);

        if (nextDayButton != null)
        {
            nextDayButton.gameObject.SetActive(true);
            nextDayButton.interactable = true;
        }
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

    private void HidePhone(TweenCallback onComplete = null)
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

            onComplete?.Invoke();
        });
    }

    private void HidePhoneAndNotification(TweenCallback onComplete = null)
    {
        Sequence seq = DOTween.Sequence();

        if (notificationGroup != null && notificationGroup.gameObject.activeSelf)
        {
            notificationGroup.DOKill();
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
                notificationGroup.gameObject.SetActive(false);

            if (phoneGroup != null)
                phoneGroup.gameObject.SetActive(false);

            onComplete?.Invoke();
        });
    }

    private void SetCanvasGroupState(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.gameObject.SetActive(visible);
        cg.alpha = visible ? 1f : 0f;
    }

    private void SetButtonState(Button button, bool enabled)
    {
        if (button == null) return;
        button.interactable = enabled;
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
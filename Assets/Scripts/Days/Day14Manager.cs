using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Day14Manager : MonoBehaviour
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
    [SerializeField] private CanvasGroup schoolBg;
    [SerializeField] private float homeFadeInDuration = 0.6f;
    [SerializeField] private float schoolFadeDuration = 0.5f;
    [SerializeField] private float delayBeforeHome = 0.1f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData midDialogue;
    [SerializeField] private DialogueData acceptDialogue;
    [SerializeField] private DialogueData declineDialogue;

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

    [Header("Random Event Popup")]
    [SerializeField] private CanvasGroup eventPopup;
    [SerializeField] private RectTransform eventPanel;
    [SerializeField] private Button acceptEventButton;
    [SerializeField] private Button declineEventButton;
    [SerializeField] private TextMeshProUGUI eventTitleText;
    [SerializeField] private TextMeshProUGUI eventDescriptionText;

    [Header("Ending")]
    [SerializeField] private CanvasGroup endReportLevelGroup;
    [SerializeField] private float endReportFadeDuration = 0.4f;
    [SerializeField] private EODReport endOfDayReportUI;

    [Header("Next Day Button")]
    [SerializeField] private Button nextDayButton;
    [SerializeField] private float nextDayButtonDelay = 3f;
    [SerializeField] private string nextSceneName = "Day 15";

    private Vector3 titleOriginalScale;
    private Vector2 phoneOriginalPos;
    private bool hasOpenedPhoneForGameplay = false;
    private bool isOpeningPhone = false;
    private bool decisionMade = false;
    private bool acceptedEvent = false;

    private RandomEventData currentRandomEvent;

    private enum Phase
    {
        IntroAnim,
        IntroDialogue,
        BudgetGameplay,
        MidDialogue,
        EventPopup,
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

        SetPopupStateInstant(eventPopup, false);

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
    }

    private void Start()
    {
        if (acceptEventButton != null)
        {
            acceptEventButton.onClick.RemoveAllListeners();
            acceptEventButton.onClick.AddListener(OnAcceptEventPressed);
        }

        if (declineEventButton != null)
        {
            declineEventButton.onClick.RemoveAllListeners();
            declineEventButton.onClick.AddListener(OnDeclineEventPressed);
        }

        if (GameManager.Instance != null && RandomEventManager.Instance != null)
        {
            currentRandomEvent = RandomEventManager.Instance.GenerateRandomEventForDay(GameManager.Instance.currentDay);

            if (currentRandomEvent != null)
                RandomEventManager.Instance.MarkEventUsed(currentRandomEvent.eventId);
        }

        PlayIntroAnim();
    }

    private void PlayIntroAnim()
    {
        currentPhase = Phase.IntroAnim;

        Sequence seq = DOTween.Sequence();

        // 1) Home background visible from the start
        if (homeBg != null)
        {
            homeBg.alpha = 1f;
            homeBg.gameObject.SetActive(true);
        }

        // 2) Intro black bg starts dimmed on top of home
        if (introBg != null)
        {
            introBg.alpha = 0.5f;                  // dim overlay
            introBg.gameObject.SetActive(true);
        }

        // 3) Title pops after a short delay, while dim overlay is still there
        if (dayTitle != null)
        {
            seq.AppendInterval(titleDelayAfterBg);
            dayTitle.localScale = Vector3.zero;
            seq.Append(dayTitle.DOScale(1f, titlePopDuration).SetEase(Ease.OutBack));
        }

        // 4) Small pause, then fade out title + black overlay
        seq.AppendInterval(0.3f);

        if (dayTitle != null)
            seq.Append(dayTitle.DOScale(0f, titleFadeOutDuration).SetEase(Ease.InBack));

        if (introBg != null)
            seq.Join(introBg.DOFade(0f, bgFadeOutDuration).SetEase(Ease.InCubic));

        // 5) Home bg stays at alpha 1; no extra fade needed here

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
        else
        {
            OnIntroDialogueFinished();
        }
    }

    private void OnIntroDialogueFinished()
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

    public void CloseFromSystemScreen()
    {
        if (currentPhase != Phase.BudgetGameplay) return;

        SetButtonState(logExpenseButton, false);
        SetButtonState(trackTabButton, false);
        SetButtonState(backButton, false);

        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);

        HidePhone(StartMidDialogue);
    }

    private void StartMidDialogue()
    {
        currentPhase = Phase.MidDialogue;

        Sequence seq = DOTween.Sequence();

        if (homeBg != null)
            seq.Append(homeBg.DOFade(0f, schoolFadeDuration).SetEase(Ease.InOutCubic));

        if (schoolBg != null)
            seq.Join(schoolBg.DOFade(1f, schoolFadeDuration).SetEase(Ease.InOutCubic));

        seq.OnComplete(() =>
        {
            if (dialogueController != null && midDialogue != null)
            {
                dialogueController.SetDialogueActive(true);
                dialogueController.inputEnabled = true;
                dialogueController.BeginDialogue(midDialogue, OnMidDialogueFinished);
            }
            else
            {
                OnMidDialogueFinished();
            }
        });
    }

    private void OnMidDialogueFinished()
    {
        ShowRandomEventPopup();
    }

    private void ShowRandomEventPopup()
    {
        currentPhase = Phase.EventPopup;
        decisionMade = false;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        if (currentRandomEvent != null)
        {
            if (eventTitleText != null)
                eventTitleText.text = currentRandomEvent.eventTitle;

            if (eventDescriptionText != null)
                eventDescriptionText.text = currentRandomEvent.description;

            // NEW: use resolver to get correct button labels (esp. for special events)
            RandomEventChoice resolvedA = RandomEventResolver.GetResolvedChoice(currentRandomEvent, true);
            RandomEventChoice resolvedB = RandomEventResolver.GetResolvedChoice(currentRandomEvent, false);

            if (acceptEventButton != null)
            {
                TextMeshProUGUI txt = acceptEventButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = !string.IsNullOrEmpty(resolvedA.buttonText) ? resolvedA.buttonText : "Choice A";
            }

            if (declineEventButton != null)
            {
                TextMeshProUGUI txt = declineEventButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = !string.IsNullOrEmpty(resolvedB.buttonText) ? resolvedB.buttonText : "Choice B";
            }
        }
        else
        {
            if (eventTitleText != null)
                eventTitleText.text = "Random Event";

            if (eventDescriptionText != null)
                eventDescriptionText.text = "Something unexpected happened today.";
        }

        ShowPopup(eventPopup, eventPanel);
    }

    private void ApplyChoice(RandomEventChoice choice)
    {
        if (choice == null || GameManager.Instance == null)
            return;

        GameManager.Instance.AddSavings(choice.savingsChange);
        GameManager.Instance.AddHappiness(choice.happinessChange);

        Debug.Log(
            $"[Random Event] Applied direct choice '{choice.buttonText}'. " +
            $"SavingsChange = {choice.savingsChange}, " +
            $"HappinessChange = {choice.happinessChange}",
            this
        );
    }

    public void OnAcceptEventPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        acceptedEvent = true;

        if (currentRandomEvent != null)
        {
            RandomEventResolver.ApplyResolvedChoice(currentRandomEvent, true);
        }

        HidePopup(eventPopup, eventPanel, StartFinalDialogue);
    }

    public void OnDeclineEventPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        acceptedEvent = false;

        if (currentRandomEvent != null)
        {
            RandomEventResolver.ApplyResolvedChoice(currentRandomEvent, false);
        }

        HidePopup(eventPopup, eventPanel, StartFinalDialogue);
    }

    private void StartFinalDialogue()
    {
        currentPhase = Phase.FinalDialogue;

        DialogueData chosenDialogue = acceptedEvent ? acceptDialogue : declineDialogue;

        if (dialogueController != null && chosenDialogue != null)
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
        if (endOfDayReportUI != null)
        {
            if (acceptedEvent)
            {
                endOfDayReportUI.SetSpecialMessage(
                    "You chose the first random event option and accepted its effects on Alex’s mood and savings."
                );
            }
            else
            {
                endOfDayReportUI.SetSpecialMessage(
                    "You chose the second random event option and lived with a different trade-off."
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

        // 1) Apply pending Day 3 debt here (Pay Later consequence)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyPendingFinalDebtIfNeeded();
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

            // 2) Decide what special message to show
            if (GameManager.Instance != null && GameManager.Instance.finalDebtApplied)
            {
                // Player had a delayed debt that was just paid
                endOfDayReportUI.SetSpecialMessage(
                    "Today, your delayed ₱500 debt from earlier was finally deducted from your savings."
                );
            }
            else
            {
                // Keep the random-event message you already set,
                // or clear if you prefer no extra text
                // endOfDayReportUI.ClearSpecialMessage(); // optional
            }

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
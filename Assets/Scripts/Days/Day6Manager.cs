using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Day6Manager : MonoBehaviour
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
    [SerializeField] private CanvasGroup classroomBg;
    [SerializeField] private float classroomFadeInDuration = 0.6f;
    [SerializeField] private float delayBeforeClassroom = 0.1f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData midDialogueBeforePhone;
    [SerializeField] private DialogueData midDialogueAfterPhone;
    [SerializeField] private DialogueData acceptDialogue;
    [SerializeField] private DialogueData declineDialogue;

    [Header("Phone UI")]
    [SerializeField] private CanvasGroup phoneGroup;
    [SerializeField] private RectTransform phoneRect;
    [SerializeField] private float phoneEaseDuration = 0.5f;
    [SerializeField] private float phoneEaseOffsetY = -300f;
    [SerializeField] private float phonePreviewDuration = 2f;

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
    [SerializeField] private string nextSceneName = "Day 7";

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
        MidDialogueBeforePhone,
        PhonePreview,
        MidDialogueAfterPhone,
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

        if (classroomBg != null)
            classroomBg.alpha = 0f;

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

        if (classroomBg != null)
        {
            seq.AppendInterval(delayBeforeClassroom);
            seq.Append(classroomBg.DOFade(1f, classroomFadeInDuration).SetEase(Ease.OutCubic));
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

        HidePhone(StartMidDialogueBeforePhone);
    }

    private void StartMidDialogueBeforePhone()
    {
        currentPhase = Phase.MidDialogueBeforePhone;

        if (dialogueController != null && midDialogueBeforePhone != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(midDialogueBeforePhone, OnMidDialogueBeforePhoneFinished);
        }
        else
        {
            OnMidDialogueBeforePhoneFinished();
        }
    }

    private void OnMidDialogueBeforePhoneFinished()
    {
        ShowPhonePreview();
    }

    private void ShowPhonePreview()
    {
        currentPhase = Phase.PhonePreview;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        if (phoneGroup != null)
            phoneGroup.interactable = false;

        AnimatePhoneIn(() =>
        {
            ShowSystemScreen();

            SetButtonState(logExpenseButton, false);
            SetButtonState(trackTabButton, false);
            SetButtonState(backButton, false);

            StartCoroutine(HidePhonePreviewAfterDelay());
        });
    }

    private IEnumerator HidePhonePreviewAfterDelay()
    {
        yield return new WaitForSeconds(phonePreviewDuration);

        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);

        HidePhone(StartMidDialogueAfterPhone);
    }

    private void StartMidDialogueAfterPhone()
    {
        currentPhase = Phase.MidDialogueAfterPhone;

        if (dialogueController != null && midDialogueAfterPhone != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(midDialogueAfterPhone, OnMidDialogueAfterPhoneFinished);
        }
        else
        {
            OnMidDialogueAfterPhoneFinished();
        }
    }

    private void OnMidDialogueAfterPhoneFinished()
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

            if (acceptEventButton != null)
            {
                TextMeshProUGUI txt = acceptEventButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = currentRandomEvent.choiceA != null ? currentRandomEvent.choiceA.buttonText : "Choice A";
            }

            if (declineEventButton != null)
            {
                TextMeshProUGUI txt = declineEventButton.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null)
                    txt.text = currentRandomEvent.choiceB != null ? currentRandomEvent.choiceB.buttonText : "Choice B";
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
            $"[Random Event] Applied choice '{choice.buttonText}'. " +
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
            ApplyChoice(currentRandomEvent.choiceA);

        HidePopup(eventPopup, eventPanel, StartFinalDialogue);
    }

    public void OnDeclineEventPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        acceptedEvent = false;

        if (currentRandomEvent != null)
            ApplyChoice(currentRandomEvent.choiceB);

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
                    "You chose the first random event option and accepted its consequences."
                );
            }
            else
            {
                endOfDayReportUI.SetSpecialMessage(
                    "You chose the second random event option and accepted a different trade-off."
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
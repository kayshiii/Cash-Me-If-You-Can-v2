using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Day8Manager : MonoBehaviour
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
    [SerializeField] private CanvasGroup roomBg;
    [SerializeField] private CanvasGroup classroomBg;
    [SerializeField] private float bgFadeDuration = 0.6f;
    [SerializeField] private float delayBeforeRoom = 0.1f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData introDialogue;
    [SerializeField] private DialogueData midDialogueRoom;
    [SerializeField] private DialogueData midDialogueClassroom;
    [SerializeField] private DialogueData finalDialogueAntacid;
    [SerializeField] private DialogueData finalDialogueClinic;

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

    [Header("Paper Scene")]
    [SerializeField] private CanvasGroup paperSceneGroup;
    [SerializeField] private RectTransform paperScenePanel;
    [SerializeField] private float paperSceneFadeDuration = 0.3f;
    [SerializeField] private float paperSceneLockedDuration = 3f;
    [SerializeField] private GameObject paperNextIndicator;

    private bool canAdvancePaperScene = false;
    private bool waitingForPaperAdvance = false;

    [Header("Decision Popup")]
    [SerializeField] private CanvasGroup decisionPopup;
    [SerializeField] private RectTransform decisionPanel;
    [SerializeField] private Button buyAntacidButton;
    [SerializeField] private Button goClinicButton;

    [Header("Costs")]
    [SerializeField] private int antacidCost = 100;

    [Header("Ending")]
    [SerializeField] private CanvasGroup endReportLevelGroup;
    [SerializeField] private float endReportFadeDuration = 0.4f;
    [SerializeField] private EODReport endOfDayReportUI;

    [Header("Next Day Button")]
    [SerializeField] private Button nextDayButton;
    [SerializeField] private float nextDayButtonDelay = 3f;
    [SerializeField] private string nextSceneName = "Day 9";

    private Vector3 titleOriginalScale;
    private Vector2 phoneOriginalPos;
    private bool hasOpenedPhoneForGameplay = false;
    private bool isOpeningPhone = false;
    private bool decisionMade = false;
    private bool choseAntacid = false;

    private enum Phase
    {
        IntroAnim,
        IntroDialogue,
        BudgetGameplay,
        MidDialogueRoom,
        PaperScene,
        MidDialogueClassroom,
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

        if (roomBg != null)
            roomBg.alpha = 0f;

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

        SetPopupStateInstant(paperSceneGroup, false);
        if (paperNextIndicator != null)
            paperNextIndicator.SetActive(false);
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
    }

    private void Update()
    {
        if (!waitingForPaperAdvance || !canAdvancePaperScene)
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            waitingForPaperAdvance = false;
            canAdvancePaperScene = false;

            if (paperNextIndicator != null)
                paperNextIndicator.SetActive(false);

            HidePaperSceneAndContinue();
        }
    }

    private void Start()
    {
        if (buyAntacidButton != null)
        {
            buyAntacidButton.onClick.RemoveAllListeners();
            buyAntacidButton.onClick.AddListener(OnBuyAntacidPressed);
        }

        if (goClinicButton != null)
        {
            goClinicButton.onClick.RemoveAllListeners();
            goClinicButton.onClick.AddListener(OnGoClinicPressed);
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

        if (roomBg != null)
        {
            seq.AppendInterval(delayBeforeRoom);
            seq.Append(roomBg.DOFade(1f, bgFadeDuration).SetEase(Ease.OutCubic));
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

        HidePhone(StartMidDialogueRoom);
    }

    private void StartMidDialogueRoom()
    {
        currentPhase = Phase.MidDialogueRoom;

        if (dialogueController != null && midDialogueRoom != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(midDialogueRoom, OnMidDialogueRoomFinished);
        }
        else
        {
            OnMidDialogueRoomFinished();
        }
    }

    private void OnMidDialogueRoomFinished()
    {
        ShowPaperScene();
    }

    private void ShowPaperScene()
    {
        currentPhase = Phase.PaperScene;

        if (paperSceneGroup == null || paperScenePanel == null)
        {
            StartMidDialogueClassroom();
            return;
        }

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        waitingForPaperAdvance = false;
        canAdvancePaperScene = false;

        if (paperNextIndicator != null)
            paperNextIndicator.SetActive(false);

        paperSceneGroup.gameObject.SetActive(true);
        paperSceneGroup.alpha = 0f;
        paperSceneGroup.interactable = false;
        paperSceneGroup.blocksRaycasts = true;

        paperScenePanel.localScale = Vector3.one * 0.9f;

        Sequence seq = DOTween.Sequence();
        seq.Append(paperSceneGroup.DOFade(1f, paperSceneFadeDuration).SetEase(Ease.OutCubic));
        seq.Join(paperScenePanel.DOScale(1f, paperSceneFadeDuration).SetEase(Ease.OutBack));
        seq.OnComplete(() =>
        {
            StartCoroutine(EnablePaperAdvanceAfterDelay());
        });
    }
    private IEnumerator EnablePaperAdvanceAfterDelay()
    {
        yield return new WaitForSeconds(paperSceneLockedDuration);

        canAdvancePaperScene = true;
        waitingForPaperAdvance = true;

        if (paperNextIndicator != null)
            paperNextIndicator.SetActive(true);
    }

    private void HidePaperSceneAndContinue()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(paperSceneGroup.DOFade(0f, paperSceneFadeDuration).SetEase(Ease.InCubic));
        seq.OnComplete(() =>
        {
            paperSceneGroup.gameObject.SetActive(false);
            SwitchToClassroom();
        });
    }

    private void SwitchToClassroom()
    {
        Sequence seq = DOTween.Sequence();

        if (roomBg != null)
            seq.Append(roomBg.DOFade(0f, bgFadeDuration).SetEase(Ease.InOutCubic));

        if (classroomBg != null)
            seq.Join(classroomBg.DOFade(1f, bgFadeDuration).SetEase(Ease.InOutCubic));

        seq.OnComplete(StartMidDialogueClassroom);
    }

    private void StartMidDialogueClassroom()
    {
        currentPhase = Phase.MidDialogueClassroom;

        if (dialogueController != null && midDialogueClassroom != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(midDialogueClassroom, OnMidDialogueClassroomFinished);
        }
        else
        {
            OnMidDialogueClassroomFinished();
        }
    }

    private void OnMidDialogueClassroomFinished()
    {
        ShowDecisionPopup();
    }

    private void ShowDecisionPopup()
    {
        currentPhase = Phase.DecisionPopup;
        decisionMade = false;
        ShowPopup(decisionPopup, decisionPanel);
    }

    public void OnBuyAntacidPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        choseAntacid = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpendFromToday(antacidCost);
            GameManager.Instance.SetDay8Decision(true);
        }

        HidePopup(decisionPopup, decisionPanel, StartFinalDialogue);
    }

    public void OnGoClinicPressed()
    {
        if (decisionMade) return;
        decisionMade = true;
        choseAntacid = false;

        if (GameManager.Instance != null)
            GameManager.Instance.SetDay8Decision(false);

        HidePopup(decisionPopup, decisionPanel, StartFinalDialogue);
    }

    private void StartFinalDialogue()
    {
        currentPhase = Phase.FinalDialogue;

        if (dialogueController == null)
        {
            OnFinalDialogueFinished();
            return;
        }

        DialogueData chosenDialogue = choseAntacid ? finalDialogueAntacid : finalDialogueClinic;

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
        if (endOfDayReportUI != null)
        {
            if (choseAntacid)
            {
                endOfDayReportUI.SetSpecialMessage(
                    "You bought antacid for ₱100 to hide the pain. It helped you keep going for now, but the real problem is still getting worse."
                );
            }
            else
            {
                endOfDayReportUI.SetSpecialMessage(
                    "You went to the clinic and got a moment to rest without spending money, but it forced Alex to confront how serious her condition really is."
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
        /*cg.interactable = visible;
        cg.blocksRaycasts = visible;*/
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
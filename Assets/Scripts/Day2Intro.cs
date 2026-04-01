using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Day2Intro : MonoBehaviour
{
    [Header("Intro BG / Title")]
    [SerializeField] private CanvasGroup introBg;
    [SerializeField] private RectTransform dayTitle;
    [SerializeField] private float bgFadeInDuration = 0.6f;
    [SerializeField] private float titlePopDuration = 0.4f;
    [SerializeField] private float titleDelayAfterBg = 0.2f;
    [SerializeField] private float titleFadeOutDuration = 0.3f;
    [SerializeField] private float bgFadeOutDuration = 0.5f;
    [SerializeField] private CanvasGroup classroomBg;
    [SerializeField] private float classroomFadeInDuration = 0.6f;
    [SerializeField] private float delayBeforeClassroom = 0.1f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData openingDialogue;
    [SerializeField] private DialogueData finalDialogue;

    [Header("Phone UI")]
    [SerializeField] private CanvasGroup phoneGroup;
    [SerializeField] private RectTransform phoneRect;
    [SerializeField] private float phoneEaseDuration = 0.5f;
    [SerializeField] private float phoneEaseOffsetY = -300f;

    [Header("App Screens")]
    [SerializeField] private CanvasGroup startScreenGroup;
    [SerializeField] private CanvasGroup popupScreenGroup;
    [SerializeField] private CanvasGroup warningScreenGroup;
    [SerializeField] private CanvasGroup systemScreenGroup;
    [SerializeField] private CanvasGroup trackScreenGroup;

    [Header("Gameplay Buttons")]
    [SerializeField] private Button logExpenseButton;
    [SerializeField] private Button trackTabButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button finishDayButton;

    [Header("Ending")]
    [SerializeField] private CanvasGroup endReportLevelGroup;
    [SerializeField] private float endReportFadeDuration = 0.4f;
    [SerializeField] private EODReport endOfDayReportUI;

    [Header("Next Day Button")]
    [SerializeField] private Button nextDayButton;
    [SerializeField] private float nextDayButtonDelay = 3f;
    [SerializeField] private string nextSceneName = "Day 3";

    private Vector3 titleOriginalScale;
    private Vector3 phoneOriginalPos;

    //fields
    private bool hasOpenedPhoneForGameplay = false;
    private bool isOpeningPhone = false;

    private enum Phase
    {
        IntroAnim,
        OpeningDialogue,
        Gameplay,
        FinalDialogue,
        Done
    }

    private Phase currentPhase = Phase.IntroAnim;

    private void Awake()
    {
        if (introBg != null) introBg.alpha = 0f;

        if (dayTitle != null)
        {
            titleOriginalScale = dayTitle.localScale;
            dayTitle.localScale = Vector3.zero;
        }

        if (classroomBg != null) classroomBg.alpha = 0f;

        if (phoneRect != null)
        {
            phoneOriginalPos = phoneRect.anchoredPosition;
            phoneRect.anchoredPosition = phoneOriginalPos + new Vector3(0f, phoneEaseOffsetY, 0f);
        }

        if (phoneGroup != null)
        {
            phoneGroup.alpha = 0f;
            phoneGroup.gameObject.SetActive(false);
        }

        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);

        if (endReportLevelGroup != null)
        {
            endReportLevelGroup.alpha = 0f;
            endReportLevelGroup.gameObject.SetActive(false);
        }

        if (nextDayButton != null)
            nextDayButton.gameObject.SetActive(false);

        /*SetButtonState(logExpenseButton, false);
        SetButtonState(trackTabButton, false);
        SetButtonState(backButton, false);
        SetButtonState(finishDayButton, false);*/
    }

    private void Start()
    {
        PlayIntroAnim();
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

        seq.OnComplete(StartOpeningDialogue);
    }

    private void StartOpeningDialogue()
    {
        currentPhase = Phase.OpeningDialogue;

        if (dialogueController != null && openingDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(openingDialogue, OnOpeningDialogueFinished);
        }
    }

    private void OnOpeningDialogueFinished()
    {
        if (hasOpenedPhoneForGameplay || isOpeningPhone) return;
        OpenPhoneForGameplay();
    }

    private void OpenPhoneForGameplay()
    {
        if (hasOpenedPhoneForGameplay || isOpeningPhone) return;

        currentPhase = Phase.Gameplay;
        isOpeningPhone = true;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        if (phoneGroup == null || phoneRect == null) return;

        phoneGroup.gameObject.SetActive(true);
        phoneGroup.alpha = 0f;
        phoneRect.anchoredPosition = phoneOriginalPos + new Vector3(0f, phoneEaseOffsetY, 0f);

        Sequence seq = DOTween.Sequence();
        seq.Append(phoneGroup.DOFade(1f, phoneEaseDuration).SetEase(Ease.OutCubic));
        seq.Join(phoneRect.DOAnchorPos(phoneOriginalPos, phoneEaseDuration).SetEase(Ease.OutBack));

        seq.OnComplete(() =>
        {
            hasOpenedPhoneForGameplay = true;
            isOpeningPhone = false;

            ShowStartScreen();

            /*SetButtonState(logExpenseButton, true);
            SetButtonState(trackTabButton, true);
            SetButtonState(backButton, true);
            SetButtonState(finishDayButton, true);*/
        });
    }

    public void ShowStartScreen()
    {
        SetCanvasGroupState(startScreenGroup, true);
        SetCanvasGroupState(popupScreenGroup, false);
        SetCanvasGroupState(warningScreenGroup, false);
        /*SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);*/
    }
    /*
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
        }*/

    public void CloseFromSystemScreen()
    {
        FinishDay();
    }

    public void FinishDay()
    {
        if (currentPhase != Phase.Gameplay) return;
        ClosePhoneAndStartFinalDialogue();
    }

    private void ClosePhoneAndStartFinalDialogue()
    {
        /*SetButtonState(logExpenseButton, false);
        SetButtonState(trackTabButton, false);
        SetButtonState(backButton, false);
        SetButtonState(finishDayButton, false);*/

        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);

        if (phoneGroup != null)
        {
            phoneGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                phoneGroup.gameObject.SetActive(false);
                StartFinalDialogue();
            });
        }
        else
        {
            StartFinalDialogue();
        }
    }

    private void StartFinalDialogue()
    {
        currentPhase = Phase.FinalDialogue;

        if (dialogueController != null && finalDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(finalDialogue, OnFinalDialogueFinished);
        }
        else
        {
            OnFinalDialogueFinished();
        }
    }

    private void OnFinalDialogueFinished()
    {
        currentPhase = Phase.Done;

        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.SetLolaMomActive(false);
            dialogueController.inputEnabled = false;
        }

        ShowEndReportLevelScreen();
    }

    private void ShowEndReportLevelScreen()
    {
        if (endReportLevelGroup == null) return;

        endReportLevelGroup.gameObject.SetActive(true);
        endReportLevelGroup.alpha = 0f;
        endReportLevelGroup.DOFade(1f, endReportFadeDuration).SetEase(Ease.OutCubic);

        if (endOfDayReportUI != null)
            endOfDayReportUI.RefreshUI();

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

    public void LoadNextDayScene()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AdvanceDay();

        SceneManager.LoadScene(nextSceneName);
    }
}
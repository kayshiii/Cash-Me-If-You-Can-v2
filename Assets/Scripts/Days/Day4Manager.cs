using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Day4Manager : MonoBehaviour
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
    [SerializeField] private DialogueData finalDialogue;

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

    [Header("Ending")]
    [SerializeField] private CanvasGroup endReportLevelGroup;
    [SerializeField] private float endReportFadeDuration = 0.4f;
    [SerializeField] private EODReport endOfDayReportUI;

    [Header("Next Day Button")]
    [SerializeField] private Button nextDayButton;
    [SerializeField] private float nextDayButtonDelay = 3f;
    [SerializeField] private string nextSceneName = "Day 5";

    private Vector3 titleOriginalScale;
    private Vector2 phoneOriginalPos;
    private bool hasOpenedPhoneForGameplay = false;
    private bool isOpeningPhone = false;

    private enum Phase
    {
        IntroAnim,
        OpeningDialogue,
        BudgetGameplay,
        SwitchToSchool,
        FinalDialogue,
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

        seq.OnComplete(StartOpeningDialogue);
    }

    private void StartOpeningDialogue()
    {
        currentPhase = Phase.OpeningDialogue;

        if (dialogueController != null && introDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(introDialogue, OnOpeningDialogueFinished);
        }
    }

    private void OnOpeningDialogueFinished()
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

        Debug.Log("[Day4] CloseFromSystemScreen called. Proceeding to final school dialogue.", this);

        SetButtonState(logExpenseButton, false);
        SetButtonState(trackTabButton, false);
        SetButtonState(backButton, false);

        SetCanvasGroupState(startScreenGroup, false);
        SetCanvasGroupState(systemScreenGroup, false);
        SetCanvasGroupState(trackScreenGroup, false);

        HidePhone(() =>
        {
            SwitchToSchool();
        });
    }

    private void SwitchToSchool()
    {
        currentPhase = Phase.SwitchToSchool;

        Sequence seq = DOTween.Sequence();

        if (homeBg != null)
            seq.Append(homeBg.DOFade(0f, schoolFadeDuration).SetEase(Ease.InOutCubic));

        if (schoolBg != null)
            seq.Join(schoolBg.DOFade(1f, schoolFadeDuration).SetEase(Ease.InOutCubic));

        seq.OnComplete(StartFinalDialogue);
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

    public void LoadNextDayScene()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AdvanceDay();

        SceneManager.LoadScene(nextSceneName);
    }
}
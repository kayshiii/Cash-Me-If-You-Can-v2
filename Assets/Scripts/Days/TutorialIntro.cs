using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static DialogueLine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialIntro : MonoBehaviour
{
    [Header("Intro BG / Title")]
    [SerializeField] private CanvasGroup tutorialBg;
    [SerializeField] private RectTransform tutorialTitle;
    [SerializeField] private float bgFadeInDuration = 0.6f;
    [SerializeField] private float titlePopDuration = 0.4f;
    [SerializeField] private float titleDelayAfterBg = 0.2f;
    [SerializeField] private float titleFadeOutDuration = 0.3f;
    [SerializeField] private float bgFadeOutDuration = 0.5f;
    [SerializeField] private CanvasGroup homeBg;
    [SerializeField] private float homeFadeInDuration = 0.6f;
    [SerializeField] private float delayBeforeHome = 0.1f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData introDialogue;      // after intro anim
    [SerializeField] private DialogueData midDialogue;        // after laptop scene

    [Header("Laptop UI")]
    [SerializeField] private CanvasGroup laptopGroup;
    [SerializeField] private RectTransform laptopRect;
    [SerializeField] private float laptopEaseDuration = 0.5f;
    [SerializeField] private float laptopEaseOffsetY = -300f;

    [Header("Laptop Screen UI")]
    [SerializeField] private CanvasGroup screenGroup;
    [SerializeField] private float screenFadeDelay = 0.2f;
    [SerializeField] private float screenFadeDuration = 0.5f;

    [Header("Phone UI")]
    [SerializeField] private CanvasGroup phoneGroup;
    [SerializeField] private RectTransform phoneRect;
    [SerializeField] private float phoneEaseDuration = 0.5f;
    [SerializeField] private float phoneEaseOffsetY = -300f;

    [Header("Phone Screens")]
    [SerializeField] private CanvasGroup imgSearching;
    [SerializeField] private CanvasGroup imgDownloading;
    [SerializeField] private CanvasGroup imgHome;
    [SerializeField] private CanvasGroup imgAppIcon;
    [SerializeField] private CanvasGroup imgAppWelcome;
    [SerializeField] private CanvasGroup imgChoose;

    [Header("Strategy Preview")]
    [SerializeField] private CanvasGroup img7030Preview;
    [SerializeField] private CanvasGroup img503020Preview;
    [SerializeField] private Button selectStrategyButton;

    [Header("Strategy Confirm Popup")]
    [SerializeField] private RectTransform strategyConfirmPanel;
    [SerializeField] private CanvasGroup strategyConfirmPopup;
    [SerializeField] private Button strategyConfirmYesButton;
    [SerializeField] private Button strategyConfirmNoButton;

    [Header("Phone Buttons")]
    [SerializeField] private GameObject btnOptionA;
    [SerializeField] private GameObject btnOptionB;

    [Header("Phone Dialogue")]
    [SerializeField] private DialogueData phoneDialogue;
    [SerializeField] private DialogueData afterPhoneDialogue;

    public DialogueData AfterPhoneDialogue => afterPhoneDialogue;

    [Header("App Screen")]
    [SerializeField] private CanvasGroup startScreenGroup;
    [SerializeField] private CanvasGroup systemScreenGroup;
    [SerializeField] private CanvasGroup trackScreenGroup;
    [SerializeField] private float systemScreenFadeDuration = 0.4f;
    [SerializeField] private float startScreenFadeDuration = 0.4f;

    [Header("Lola/Mom Spotlight")]
    [SerializeField] private CanvasGroup spotlightGroup;
    [SerializeField] private DialogueData lolaMomIntroDialogue;

    [Header("Tutorial Controlled Buttons")]
    [SerializeField] private Button logExpenseButton;
    [SerializeField] private Button trackTabButton;
    [SerializeField] private Button needsTabButton;
    [SerializeField] private Button wantsTabButton;
    [SerializeField] private Button BackButton;
    [SerializeField] private Button confirmAllocationButton;
    [SerializeField] private Button[] lunchChoiceButtons;
    [SerializeField] private Button[] commuteChoiceButtons;
    [SerializeField] private Button[] wantChoiceButtons;

    [Header("Ending")]
    [SerializeField] private DialogueData finalDialogue;
    [SerializeField] private CanvasGroup endReportLevelGroup;
    [SerializeField] private float endReportFadeDuration = 0.4f;

    [Header("Script")]
    [SerializeField] private EODReport endOfDayReportUI;

    [Header("Next Day Button")]
    [SerializeField] private Button nextDayButton;
    [SerializeField] private float nextDayButtonDelay = 3f;
    [SerializeField] private string nextSceneName = "Day 2";

    private Vector3 phoneOriginalPos;

    [Header("Read Indicator")]
    [SerializeField] private GameObject readIndicator;

    [Header("HUD")]
    [SerializeField] private HappinessMeter happinessMeter;

    private Vector3 titleOriginalScale;
    private Vector3 laptopOriginalPos;

    // fields
    private bool canContinueFromLaptop = false;
    private bool phoneChoiceMade = false;
    private bool shouldClosePhoneAfterLolaDialogue = false;

    private enum Phase { IntroAnim, IntroDialogue, LaptopScene, MidDialogue, PhoneScene, WaitingForPhoneChoice, AfterPhoneDialogue, LolaMomDialogue, Done }
    private Phase currentPhase = Phase.IntroAnim;

    private enum PendingStrategy
    {
        None,
        SeventyThirty,
        FiftyThirtyTwenty
    }

    private PendingStrategy pendingStrategy = PendingStrategy.None;

    private void Awake()
    {
        if (tutorialBg != null) tutorialBg.alpha = 0f;

        if (tutorialTitle != null)
        {
            titleOriginalScale = tutorialTitle.localScale;
            tutorialTitle.localScale = Vector3.zero;
        }

        if (homeBg != null) homeBg.alpha = 0f;

        if (laptopRect != null)
        {
            laptopOriginalPos = laptopRect.anchoredPosition;
            laptopRect.anchoredPosition = laptopOriginalPos + new Vector3(0f, laptopEaseOffsetY, 0f);
        }

        if (laptopGroup != null)
        {
            laptopGroup.alpha = 0f;
            laptopGroup.gameObject.SetActive(false);
        }

        if (screenGroup != null)
        {
            screenGroup.alpha = 0f;
            screenGroup.gameObject.SetActive(false);
        }

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

        SetCanvasGroupVisible(img7030Preview, false);
        SetCanvasGroupVisible(img503020Preview, false);

        if (selectStrategyButton != null)
        {
            selectStrategyButton.gameObject.SetActive(false);
            selectStrategyButton.interactable = false;
        }

        SetPopupStateInstant(strategyConfirmPopup, false);

        if (startScreenGroup != null)
        {
            startScreenGroup.alpha = 0f;
            startScreenGroup.gameObject.SetActive(false);
        }

        if (systemScreenGroup != null)
        {
            systemScreenGroup.alpha = 0f;
            systemScreenGroup.gameObject.SetActive(false);
        }

        if (trackScreenGroup != null)
        {
            trackScreenGroup.alpha = 0f;
            trackScreenGroup.gameObject.SetActive(false);
        }

        if (endReportLevelGroup != null)
        {
            endReportLevelGroup.alpha = 0f;
            endReportLevelGroup.gameObject.SetActive(false);
        }

        if (nextDayButton != null)
        {
            nextDayButton.gameObject.SetActive(false);
        }

        SetCanvasGroupVisible(imgSearching, false);
        SetCanvasGroupVisible(imgDownloading, false);
        SetCanvasGroupVisible(imgHome, false);
        SetCanvasGroupVisible(imgAppIcon, false);
        SetCanvasGroupVisible(imgAppWelcome, false);
        SetCanvasGroupVisible(imgChoose, false);

        if (btnOptionA != null) btnOptionA.SetActive(false);
        if (btnOptionB != null) btnOptionB.SetActive(false);

        DisableAllTutorialButtons();

        if (readIndicator != null) readIndicator.SetActive(false);
    }

    private void SetCanvasGroupVisible(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.gameObject.SetActive(true);
        cg.alpha = visible ? 1f : 0f;
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

    private void HidePopup(CanvasGroup popup, RectTransform panel)
    {
        if (popup == null || panel == null) return;

        popup.DOKill();
        panel.DOKill();

        popup.interactable = false;
        popup.blocksRaycasts = false;

        popup.DOFade(0f, 0.18f).SetEase(Ease.InCubic);
        panel.DOScale(0.85f, 0.18f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                popup.gameObject.SetActive(false);
            });
    }

    private void Start()
    {
        if (happinessMeter != null)
            happinessMeter.UpdateVisual();

        if (selectStrategyButton != null)
        {
            selectStrategyButton.onClick.RemoveAllListeners();
            selectStrategyButton.onClick.AddListener(OnSelectStrategyPressed);
        }

        if (strategyConfirmYesButton != null)
        {
            strategyConfirmYesButton.onClick.RemoveAllListeners();
            strategyConfirmYesButton.onClick.AddListener(OnStrategyConfirmYesPressed);
        }

        if (strategyConfirmNoButton != null)
        {
            strategyConfirmNoButton.onClick.RemoveAllListeners();
            strategyConfirmNoButton.onClick.AddListener(OnStrategyConfirmNoPressed);
        }

        PlayIntroAnim();
    }

    private void Update()
    {
        if (currentPhase == Phase.LaptopScene && canContinueFromLaptop)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                ProceedFromLaptopToMidDialogue();
            }
        }
    }

    // ====== PHASE 1: INTRO ANIM ======
    private void PlayIntroAnim()
    {
        currentPhase = Phase.IntroAnim;

        Sequence seq = DOTween.Sequence();

        if (tutorialBg != null)
        {
            seq.Append(tutorialBg.DOFade(1f, bgFadeInDuration).SetEase(Ease.OutCubic));
        }

        if (tutorialTitle != null)
        {
            seq.AppendInterval(titleDelayAfterBg);
            tutorialTitle.localScale = Vector3.zero;
            seq.Append(tutorialTitle.DOScale(1f, titlePopDuration).SetEase(Ease.OutBack));
        }

        if (tutorialTitle != null || tutorialBg != null)
        {
            seq.AppendInterval(0.3f);

            if (tutorialTitle != null)
                seq.Append(tutorialTitle.DOScale(0f, titleFadeOutDuration).SetEase(Ease.InBack));

            if (tutorialBg != null)
                seq.Join(tutorialBg.DOFade(0f, bgFadeOutDuration).SetEase(Ease.InCubic));
        }

        if (homeBg != null)
        {
            seq.AppendInterval(delayBeforeHome);
            seq.Append(homeBg.DOFade(1f, homeFadeInDuration).SetEase(Ease.OutCubic));
        }

        seq.OnComplete(() =>
        {
            StartIntroDialogue();
        });
    }

    // ====== PHASE 2: INTRO DIALOGUE ======
    private void StartIntroDialogue()
    {
        currentPhase = Phase.IntroDialogue;

        if (dialogueController != null && introDialogue != null)
        {
            dialogueController.BeginDialogue(introDialogue, OnIntroDialogueFinished);
        }
    }

    private void OnIntroDialogueFinished()
    {
        PlayLaptopScene();
    }

    // ====== PHASE 3: LAPTOP SCENE ======
    private void PlayLaptopScene()
    {
        currentPhase = Phase.LaptopScene;
        canContinueFromLaptop = false;
        if (readIndicator != null) readIndicator.SetActive(false);

        if (dialogueController != null)
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;

        if (laptopGroup == null || laptopRect == null) return;

        laptopGroup.gameObject.SetActive(true);
        laptopGroup.alpha = 0f;
        laptopRect.anchoredPosition = laptopOriginalPos + new Vector3(0f, laptopEaseOffsetY, 0f);

        Sequence seq = DOTween.Sequence();
        seq.Append(laptopGroup.DOFade(1f, laptopEaseDuration).SetEase(Ease.OutCubic));
        seq.Join(laptopRect.DOAnchorPos(laptopOriginalPos, laptopEaseDuration).SetEase(Ease.OutBack));

        if (screenGroup != null)
        {
            screenGroup.gameObject.SetActive(true);
            screenGroup.alpha = 0f;

            seq.AppendInterval(screenFadeDelay);
            seq.Append(screenGroup.DOFade(1f, screenFadeDuration).SetEase(Ease.OutCubic));
        }

        // After laptop + screen fully visible, wait 9s, then show indicator
        seq.OnComplete(() =>
        {
            StartCoroutine(EnableLaptopContinueAfterDelay(9f));
        });
    }

    private IEnumerator EnableLaptopContinueAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    canContinueFromLaptop = true;
    if (readIndicator != null) readIndicator.SetActive(true);
}

    private void ProceedFromLaptopToMidDialogue()
    {
        canContinueFromLaptop = false;
        if (readIndicator != null) readIndicator.SetActive(false);

        if (laptopGroup != null)
        {
            laptopGroup.DOFade(0f, 0.4f).OnComplete(() =>
            {
                laptopGroup.gameObject.SetActive(false);
            });
        }

        StartMidDialogue();
    }

    // ====== PHASE 4: MID DIALOGUE ======
    private void StartMidDialogue()
    {
        currentPhase = Phase.MidDialogue;

        if (dialogueController != null && midDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;     // re‑enable
            dialogueController.BeginDialogue(midDialogue, OnMidDialogueFinished);
        }
    }

    private void OnMidDialogueFinished()
    {
        currentPhase = Phase.MidDialogue; // this was the previous
                                          // Now go to phone scene
        PlayPhoneScene();
    }

    private void PlayPhoneScene()
    {
        currentPhase = Phase.PhoneScene;

        if (dialogueController != null)
            dialogueController.SetDialogueActive(false); // hide main dialogue UI during phone
            dialogueController.inputEnabled = false;

        if (readIndicator != null) readIndicator.SetActive(false);
        if (phoneGroup == null || phoneRect == null) return;

        phoneGroup.gameObject.SetActive(true);
        phoneGroup.alpha = 0f;
        phoneRect.anchoredPosition = phoneOriginalPos + new Vector3(0f, phoneEaseOffsetY, 0f);

        // Reset all screens
        SetCanvasGroupVisible(imgSearching, false);
        SetCanvasGroupVisible(imgDownloading, false);
        SetCanvasGroupVisible(imgHome, false);
        SetCanvasGroupVisible(imgAppIcon, false);
        SetCanvasGroupVisible(imgAppWelcome, false);
        SetCanvasGroupVisible(imgChoose, false);
        if (btnOptionA != null) btnOptionA.SetActive(false);
        if (btnOptionB != null) btnOptionB.SetActive(false);

        SetCanvasGroupVisible(img7030Preview, false);
        SetCanvasGroupVisible(img503020Preview, false);

        pendingStrategy = PendingStrategy.None;

        if (selectStrategyButton != null)
        {
            selectStrategyButton.gameObject.SetActive(false);
            selectStrategyButton.interactable = false;
        }

        Sequence seq = DOTween.Sequence();

        // 1) Phone ease in
        seq.Append(phoneGroup.DOFade(1f, phoneEaseDuration).SetEase(Ease.OutCubic));
        seq.Join(phoneRect.DOAnchorPos(phoneOriginalPos, phoneEaseDuration).SetEase(Ease.OutBack));

        // 2) Show "searching"
        if (imgSearching != null)
        {
            seq.AppendCallback(() => SetCanvasGroupVisible(imgSearching, true));
        }

        // 3) After 1 sec, show "downloading"
        //if (imgDownloading != null)
        //{
        //    seq.AppendInterval(1f);
        //    seq.AppendCallback(() =>
        //    {
        //        SetCanvasGroupVisible(imgDownloading, true);
        //    });
        //}

        //// 4) Then home screen
        //if (imgHome != null)
        //{
        //    seq.AppendInterval(0.8f);
        //    seq.AppendCallback(() =>
        //    {
        //        SetCanvasGroupVisible(imgSearching, false);
        //        SetCanvasGroupVisible(imgDownloading, false);
        //        SetCanvasGroupVisible(imgHome, true);
        //    });
        //}

        //// 5) App icon
        //if (imgAppIcon != null)
        //{
        //    seq.AppendInterval(0.6f);
        //    seq.AppendCallback(() =>
        //    {
        //        SetCanvasGroupVisible(imgAppIcon, true);
        //    });
        //}

        //// 6) App welcome
        //if (imgAppWelcome != null)
        //{
        //    seq.AppendInterval(0.5f);
        //    seq.AppendCallback(() =>
        //    {
        //        SetCanvasGroupVisible(imgHome, false);
        //        SetCanvasGroupVisible(imgAppIcon, false);
        //        SetCanvasGroupVisible(imgAppWelcome, true);
        //    });
        //}

        // 7) Choose screen
        if (imgChoose != null)
        {
            seq.AppendInterval(8f);
            seq.AppendCallback(() =>
            {
                SetCanvasGroupVisible(imgSearching, false);
                //SetCanvasGroupVisible(imgAppWelcome, false);
                SetCanvasGroupVisible(imgChoose, true);
            });
        }

        // 8) Fade/pop in buttons
        seq.AppendInterval(0.3f);
        seq.AppendCallback(() =>
        {
            if (btnOptionA != null)
            {
                btnOptionA.SetActive(true);
                var rtA = btnOptionA.GetComponent<RectTransform>();
                if (rtA != null)
                {
                    rtA.localScale = Vector3.zero;
                    rtA.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                }
            }

            if (btnOptionB != null)
            {
                btnOptionB.SetActive(true);
                var rtB = btnOptionB.GetComponent<RectTransform>();
                if (rtB != null)
                {
                    rtB.localScale = Vector3.zero;
                    rtB.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
                }
            }
        });

        // When the visual sequence ends, just mark that we're waiting
        seq.OnComplete(() =>
        {
            currentPhase = Phase.WaitingForPhoneChoice;
            phoneChoiceMade = false;
            // No dialogue yet; just sit on the choose screen
        });
    }

    public void OnPhoneChoice7030()
    {
        if (currentPhase != Phase.WaitingForPhoneChoice) return;

        pendingStrategy = PendingStrategy.SeventyThirty;
        ShowStrategyPreview(img7030Preview);

        if (selectStrategyButton != null)
        {
            selectStrategyButton.gameObject.SetActive(true);
            selectStrategyButton.interactable = true;
        }
    }

    public void OnPhoneChoice503020()
    {
        if (currentPhase != Phase.WaitingForPhoneChoice) return;

        pendingStrategy = PendingStrategy.FiftyThirtyTwenty;
        ShowStrategyPreview(img503020Preview);

        if (selectStrategyButton != null)
        {
            selectStrategyButton.gameObject.SetActive(true);
            selectStrategyButton.interactable = true;
        }
    }

    public void OnSelectStrategyPressed()
    {
        if (currentPhase != Phase.WaitingForPhoneChoice) return;
        if (pendingStrategy == PendingStrategy.None) return;

        ShowPopup(strategyConfirmPopup, strategyConfirmPanel);
    }

    public void OnStrategyConfirmYesPressed()
    {
        HidePopup(strategyConfirmPopup, strategyConfirmPanel);

        phoneChoiceMade = true;

        if (GameManager.Instance != null)
        {
            switch (pendingStrategy)
            {
                case PendingStrategy.SeventyThirty:
                    GameManager.Instance.SetBudgetType(BudgetType.SeventyThirty);
                    break;

                case PendingStrategy.FiftyThirtyTwenty:
                    GameManager.Instance.SetBudgetType(BudgetType.FiftyThirtyTwenty);
                    break;
            }
        }

        StartPhoneDialogue();
    }
    private void ShowStrategyPreview(CanvasGroup target)
    {
        if (target == null) return;

        SetCanvasGroupVisible(img7030Preview, false);
        SetCanvasGroupVisible(img503020Preview, false);

        target.gameObject.SetActive(true);
        target.alpha = 0f;

        RectTransform rt = target.GetComponent<RectTransform>();
        if (rt != null)
            rt.localScale = Vector3.one * 0.9f;

        target.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);

        if (rt != null)
            rt.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
    public void OnStrategyConfirmNoPressed()
    {
        HidePopup(strategyConfirmPopup, strategyConfirmPanel);
    }

    private void StartPhoneDialogue()
    {
        currentPhase = Phase.AfterPhoneDialogue;

        SetCanvasGroupVisible(img7030Preview, false);
        SetCanvasGroupVisible(img503020Preview, false);

        if (selectStrategyButton != null)
        {
            selectStrategyButton.gameObject.SetActive(false);
            selectStrategyButton.interactable = false;
        }

        if (btnOptionA != null) btnOptionA.SetActive(false);
        if (btnOptionB != null) btnOptionB.SetActive(false);

        if (imgChoose != null)
            SetCanvasGroupVisible(imgChoose, false);

        if (phoneGroup != null)
        {
            phoneGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                phoneGroup.gameObject.SetActive(false);
            });
        }

        if (dialogueController != null && phoneDialogue != null)
        {
            // Show the dialogue UI again, but phone stays on screen
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(phoneDialogue, OnPhoneDialogueFinished);
        }
    }

    private void OnPhoneDialogueFinished()
    {
        // PhoneDialogue done → fade in system screen, then play afterPhoneDialogue
        ShowSystemScreenThenAfterPhone();
    }

    private void ShowSystemScreenThenAfterPhone()
    {
        if (systemScreenGroup != null)
        {
            systemScreenGroup.gameObject.SetActive(true);
            systemScreenGroup.alpha = 0f;
            systemScreenGroup
                .DOFade(1f, systemScreenFadeDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (dialogueController != null && afterPhoneDialogue != null)
                    {
                        dialogueController.SetDialogueActive(true);
                        dialogueController.inputEnabled = true;
                        dialogueController.BeginDialogue(afterPhoneDialogue, OnAfterPhoneDialogueFinished);
                    }
                });
        }
        else
        {
            if (dialogueController != null && afterPhoneDialogue != null)
            {
                dialogueController.SetDialogueActive(true);
                dialogueController.inputEnabled = true;
                dialogueController.BeginDialogue(afterPhoneDialogue, OnAfterPhoneDialogueFinished);
            }
        }
    }

    private void OnAfterPhoneDialogueFinished()
    {
        // Turn phone + system screen ON here
        if (phoneGroup != null)
        {
            phoneGroup.gameObject.SetActive(true);
            phoneGroup.alpha = 1f;          // or DOFade if you want
        }

        if (systemScreenGroup != null)
        {
            systemScreenGroup.gameObject.SetActive(true);
            systemScreenGroup.alpha = 1f;
        }

        // Now start Lola intro dialogue
        StartLolaIntroDialogue();
    }

    private void StartLolaIntroDialogue()
    {
        if (dialogueController != null && lolaMomIntroDialogue != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(lolaMomIntroDialogue, OnLolaIntroFinished);
        }
    }

    private void OnLolaIntroFinished()
    {
        if (dialogueController != null)
        {
            dialogueController.SetLolaMomActive(false);
            dialogueController.inputEnabled = false;

            if (readIndicator != null) readIndicator.SetActive(false);

            if (spotlightGroup != null)
            {
                spotlightGroup.alpha = 0f;
                spotlightGroup.gameObject.SetActive(false);
            }
        }

        if (phoneGroup != null)
        {
            phoneGroup.gameObject.SetActive(true);
            phoneGroup.alpha = 1f;
        }

        if (systemScreenGroup != null)
        {
            systemScreenGroup.gameObject.SetActive(true);
            systemScreenGroup.alpha = 1f;
        }

        if (shouldClosePhoneAfterLolaDialogue)
        {
            shouldClosePhoneAfterLolaDialogue = false;
            ClosePhoneAndEndTutorial();
            return;
        }

        currentPhase = Phase.Done;
    }
    private void SetButtonState(Button button, bool enabled)
    {
        if (button == null) return;
        button.interactable = enabled;
    }

    private void SetButtonState(Button[] buttons, bool enabled)
    {
        if (buttons == null) return;

        foreach (Button button in buttons)
        {
            if (button == null) continue;
            button.interactable = enabled;
        }
    }

    private void DisableAllTutorialButtons()
    {
        SetButtonState(logExpenseButton, false);
        SetButtonState(trackTabButton, false);
        SetButtonState(needsTabButton, false);
        SetButtonState(wantsTabButton, false);
        SetButtonState(confirmAllocationButton, false);

        SetButtonState(lunchChoiceButtons, false);
        SetButtonState(commuteChoiceButtons, false);
        SetButtonState(wantChoiceButtons, false);
    }

    private void EnableOnly(params Button[] buttons)
    {
        DisableAllTutorialButtons();

        if (buttons == null) return;

        foreach (Button btn in buttons)
        {
            SetButtonState(btn, true);
        }
    }
    public void OnLolaStepStarted(DialogueLine line)
    {
        switch (line.lolaStep)
        {
            case LolaStep.ShowStartIntro:
                ShowStartScreen();
                break;

            case LolaStep.ExplainLogExpense:
                ShowStartScreen();
                DisableAllTutorialButtons();
                break;

            case LolaStep.SystemScreen:
                ShowSystemScreen();
                DisableAllTutorialButtons();
                break;

            case LolaStep.ExplainChoices:
                ShowSystemScreen();
                DisableAllTutorialButtons();
                break;

            case LolaStep.WantsSelection:
                ShowSystemScreen();
                DisableAllTutorialButtons();
                //SetButtonState(wantsTabButton, true);
                break;

            case LolaStep.WantsChoices:
                ShowSystemScreen();
                DisableAllTutorialButtons();
                break;

            case LolaStep.Confirm:
                ShowSystemScreen();
                DisableAllTutorialButtons();
                SetButtonState(confirmAllocationButton, true);
                break;

            case LolaStep.BackToStart:
                ShowStartScreen();
                DisableAllTutorialButtons();
                break;

            case LolaStep.ExplainTrack:
                ShowTrackScreen();
                DisableAllTutorialButtons();
                //EnableOnly(BackButton);
                break;

            case LolaStep.ExitApp:
                DisableAllTutorialButtons();
                shouldClosePhoneAfterLolaDialogue = true;
                break;
        }
    }

    public void EnableButtonForLolaStep(DialogueLine.LolaStep step)
    {
        switch (step)
        {
            case DialogueLine.LolaStep.SystemScreen:
                DisableAllTutorialButtons();
                break;

            case DialogueLine.LolaStep.ExplainLogExpense:
                EnableOnly(logExpenseButton);
                break;

            case DialogueLine.LolaStep.ExplainChoices:
                DisableAllTutorialButtons();
                SetButtonState(lunchChoiceButtons, true);
                SetButtonState(commuteChoiceButtons, true);
                break;

            case DialogueLine.LolaStep.WantsSelection:
                EnableOnly(wantsTabButton);
                break;

            case DialogueLine.LolaStep.WantsChoices:
                DisableAllTutorialButtons();
                SetButtonState(wantChoiceButtons, true);
                break;

            case DialogueLine.LolaStep.Confirm:
                EnableOnly(confirmAllocationButton);
                break;

            case DialogueLine.LolaStep.BackToStart:
                EnableOnly(trackTabButton);
                break;

            case DialogueLine.LolaStep.ExplainTrack:
                DisableAllTutorialButtons();
                SetButtonState(trackTabButton, true);
                SetButtonState(BackButton, true);
                break;
        }
    }

    private void ShowStartScreen()
    {
        if (phoneGroup != null)
        {
            phoneGroup.gameObject.SetActive(true);
            phoneGroup.alpha = 1f;
        }

        if (startScreenGroup != null)
        {
            startScreenGroup.gameObject.SetActive(true);
            startScreenGroup.alpha = 1f;
        }

        if (systemScreenGroup != null)
        {
            systemScreenGroup.gameObject.SetActive(false);
            systemScreenGroup.alpha = 0f;
        }

        if (trackScreenGroup != null)
        {
            trackScreenGroup.gameObject.SetActive(false);
            trackScreenGroup.alpha = 0f;
        }
    }

    private void ShowSystemScreen()
    {
        if (phoneGroup != null)
        {
            phoneGroup.gameObject.SetActive(true);
            phoneGroup.alpha = 1f;
        }

        if (startScreenGroup != null)
        {
            startScreenGroup.gameObject.SetActive(false);
            startScreenGroup.alpha = 0f;
        }

        if (systemScreenGroup != null)
        {
            systemScreenGroup.gameObject.SetActive(true);
            systemScreenGroup.alpha = 1f;
        }

        if (trackScreenGroup != null)
        {
            trackScreenGroup.gameObject.SetActive(false);
            trackScreenGroup.alpha = 0f;
        }
    }

    private void ShowTrackScreen()
    {
        if (phoneGroup != null)
        {
            phoneGroup.gameObject.SetActive(true);
            phoneGroup.alpha = 1f;
        }

        if (startScreenGroup != null)
        {
            startScreenGroup.gameObject.SetActive(false);
            startScreenGroup.alpha = 0f;
        }

        if (systemScreenGroup != null)
        {
            systemScreenGroup.gameObject.SetActive(false);
            systemScreenGroup.alpha = 0f;
        }

        if (trackScreenGroup != null)
        {
            trackScreenGroup.gameObject.SetActive(true);
            trackScreenGroup.alpha = 1f;
        }
    }

    private void ClosePhoneAndEndTutorial()
    {
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

        if (startScreenGroup != null)
        {
            startScreenGroup.gameObject.SetActive(false);
            startScreenGroup.alpha = 0f;
        }

        if (systemScreenGroup != null)
        {
            systemScreenGroup.gameObject.SetActive(false);
            systemScreenGroup.alpha = 0f;
        }

        if (trackScreenGroup != null)
        {
            trackScreenGroup.gameObject.SetActive(false);
            trackScreenGroup.alpha = 0f;
        }

        if (dialogueController != null)
        {
            dialogueController.SetLolaMomActive(false);
            dialogueController.inputEnabled = true;
        }

        if (readIndicator != null) readIndicator.SetActive(false);

        if (spotlightGroup != null)
        {
            spotlightGroup.alpha = 0f;
            spotlightGroup.gameObject.SetActive(false);
        }
    }

    private void StartFinalDialogue()
    {
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
        {
            GameManager.Instance.AdvanceDay();
        }

        SceneManager.LoadScene(nextSceneName);
    }
}

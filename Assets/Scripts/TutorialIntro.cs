using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    [Header("Phone Buttons")]
    [SerializeField] private GameObject btnOptionA;
    [SerializeField] private GameObject btnOptionB;

    [Header("Phone Dialogue")]
    [SerializeField] private DialogueData phoneDialogue; // short notification-like lines shown while phone is visible
    [SerializeField] private DialogueData nextDialogueAfterPhone; // short notification-like lines shown while phone is visible

    [Header("Lola/Mom Spotlight")]
    [SerializeField] private CanvasGroup spotlightGroup;
    [SerializeField] private DialogueData lolaMomDialogue;

    private Vector3 phoneOriginalPos;

    [Header("Read Indicator")]
    [SerializeField] private GameObject readIndicator;

    private Vector3 titleOriginalScale;
    private Vector3 laptopOriginalPos;

    // fields
    private bool canContinueFromLaptop = false;
    private bool phoneChoiceMade = false;

    private enum Phase { IntroAnim, IntroDialogue, LaptopScene, MidDialogue, PhoneScene, WaitingForPhoneChoice, AfterPhoneDialogue, LolaMomDialogue, Done }
    private Phase currentPhase = Phase.IntroAnim;


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

        SetCanvasGroupVisible(imgSearching, false);
        SetCanvasGroupVisible(imgDownloading, false);
        SetCanvasGroupVisible(imgHome, false);
        SetCanvasGroupVisible(imgAppIcon, false);
        SetCanvasGroupVisible(imgAppWelcome, false);
        SetCanvasGroupVisible(imgChoose, false);

        if (btnOptionA != null) btnOptionA.SetActive(false);
        if (btnOptionB != null) btnOptionB.SetActive(false);

        if (readIndicator != null) readIndicator.SetActive(false);
    }

    private void SetCanvasGroupVisible(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;
        cg.gameObject.SetActive(true);
        cg.alpha = visible ? 1f : 0f;
    }

    private void Start()
    {
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

        // After laptop + screen fully visible, wait 2s, then show indicator
        seq.OnComplete(() =>
        {
            StartCoroutine(EnableLaptopContinueAfterDelay(2f));
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
        if (imgDownloading != null)
        {
            seq.AppendInterval(1f);
            seq.AppendCallback(() =>
            {
                SetCanvasGroupVisible(imgDownloading, true);
            });
        }

        // 4) Then home screen
        if (imgHome != null)
        {
            seq.AppendInterval(0.8f);
            seq.AppendCallback(() =>
            {
                SetCanvasGroupVisible(imgSearching, false);
                SetCanvasGroupVisible(imgDownloading, false);
                SetCanvasGroupVisible(imgHome, true);
            });
        }

        // 5) App icon
        if (imgAppIcon != null)
        {
            seq.AppendInterval(0.6f);
            seq.AppendCallback(() =>
            {
                SetCanvasGroupVisible(imgAppIcon, true);
            });
        }

        // 6) App welcome
        if (imgAppWelcome != null)
        {
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() =>
            {
                SetCanvasGroupVisible(imgHome, false);
                SetCanvasGroupVisible(imgAppIcon, false);
                SetCanvasGroupVisible(imgAppWelcome, true);
            });
        }

        // 7) Choose screen
        if (imgChoose != null)
        {
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() =>
            {
                SetCanvasGroupVisible(imgAppWelcome, false);
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

    public void OnPhoneChoiceA()
    {
        if (currentPhase != Phase.WaitingForPhoneChoice) return;

        phoneChoiceMade = true;
        // TODO: store choice in a GameManager if needed (70/30)
        StartPhoneDialogue();
    }

    public void OnPhoneChoiceB()
    {
        if (currentPhase != Phase.WaitingForPhoneChoice) return;

        phoneChoiceMade = true;
        // TODO: store choice in a GameManager if needed (50/30/20)
        StartPhoneDialogue();
    }

    private void StartPhoneDialogue()
    {
        currentPhase = Phase.AfterPhoneDialogue;

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
        if (dialogueController != null && phoneDialogue != null)
        {
            // Show the dialogue UI again, but phone stays on screen
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(nextDialogueAfterPhone, StartLolaMomDialogue);
        }

        StartLolaMomDialogue();
    }

    private void StartLolaMomDialogue()
    {
        currentPhase = Phase.LolaMomDialogue;

        // Fade in spotlight overlay
        if (spotlightGroup != null)
        {
            spotlightGroup.gameObject.SetActive(true);
            spotlightGroup.alpha = 0f;
            spotlightGroup.DOFade(1f, 0.4f).SetEase(Ease.OutCubic);
        }

        // Start the Lola/Mom dialogue; Space/left click behavior is the same
        if (dialogueController != null && lolaMomDialogue != null)
        {
            dialogueController.SetDialogueActive(true);   // show your dialogue bubbles
            dialogueController.inputEnabled = true;       // Space/left click works

            dialogueController.BeginDialogue(lolaMomDialogue, OnLolaMomDialogueFinished);
        }
    }

    private void OnLolaMomDialogueFinished()
    {
        // Hide spotlight
        if (spotlightGroup != null)
        {
            spotlightGroup.DOFade(0f, 0.3f).OnComplete(() =>
            {
                spotlightGroup.gameObject.SetActive(false);
            });
        }

        currentPhase = Phase.Done;
        // TODO: start main gameplay, next scene, or next dialogue block here
    }
    private void OnFinalDialogueFinished()
    {
        currentPhase = Phase.Done;
        // Then you can start the next main dialogue block or gameplay
        // Example: Start another DialogueData if you have one:
        //dialogueController.BeginDialogue(nextDialogueAfterPhone, OnFinalDialogueFinished);
    }
}

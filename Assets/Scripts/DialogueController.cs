using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private DialogueData dialogueData;
    [SerializeField] private SystemScreenController systemScreenController;

    [Header("Dialogue Proper")]
    [SerializeField] private GameObject dialogueRoot;

    [Header("Chat Log")]
    [SerializeField] private ChatLogManager chatLogManager;

    [Header("Narrator UI")]
    [SerializeField] private CanvasGroup narratorGroup;
    [SerializeField] private TextMeshProUGUI narratorText;
    [SerializeField] private float narratorPopDuration = 0.3f;

    [Header("Alex UI")]
    [SerializeField] private RectTransform alexRect;
    [SerializeField] private CanvasGroup alexBubbleGroup;
    [SerializeField] private TextMeshProUGUI alexText;
    [SerializeField] private float alexSlideDuration = 0.4f;
    [SerializeField] private float alexSlideOffset = -300f;
    [SerializeField] private float alexBubblePopDuration = 0.25f;
    private Tween alexMoveTween;
    private Tween alexBubbleTween;

    [Header("Alex Internal UI")]
    [SerializeField] private RectTransform alexInternalRect;
    [SerializeField] private CanvasGroup alexInternalBubbleGroup;
    [SerializeField] private TextMeshProUGUI alexInternalText;
    [SerializeField] private float alexInternalSlideDuration = 0.4f;
    [SerializeField] private float alexInternalSlideOffset = 300f;
    [SerializeField] private float alexInternalBubblePopDuration = 0.25f;
    private Tween alexInternalMoveTween;
    private Tween alexInternalBubbleTween;
    private Vector2 alexInternalOriginalPos;

    [Header("Notification UI")]
    [SerializeField] private CanvasGroup notifGroup;
    [SerializeField] private RectTransform notifRect;
    [SerializeField] private TextMeshProUGUI notifText;
    [SerializeField] private float notifSlideDuration = 0.35f;
    [SerializeField] private float notifSlideOffset = -400f;

    [Header("Lola Mom UI")]
    [SerializeField] private RectTransform lolaMomRect;
    [SerializeField] private CanvasGroup lolaMomBubbleGroup;
    [SerializeField] private TextMeshProUGUI lolaMomText;
    [SerializeField] private float lolaMomPopDuration = 0.3f;
    [SerializeField] private float lolaMomSlideDuration = 0.4f;
    [SerializeField] private float lolaMomSlideOffset = -300f;
    private Coroutine hideBubbleCoroutine;
    private Tween lolaMomBubbleHideTween;
    private Tween lolaMomMoveTween;
    private Vector2 lolaMomOriginalPos;
    private bool waitingForButton = false;

    [Header("Boyet UI")]
    [SerializeField] private RectTransform boyetRect;
    [SerializeField] private CanvasGroup boyetBubbleGroup;
    [SerializeField] private TextMeshProUGUI boyetText;
    [SerializeField] private float boyetSlideDuration = 0.4f;
    [SerializeField] private float boyetSlideOffset = 300f;
    [SerializeField] private float boyetBubblePopDuration = 0.25f;
    private Tween boyetMoveTween;
    private Tween boyetBubbleTween;
    private Vector2 boyetOriginalPos;

    [Header("Nurse UI")]
    [SerializeField] private RectTransform nurseRect;
    [SerializeField] private CanvasGroup nurseBubbleGroup;
    [SerializeField] private TextMeshProUGUI nurseText;
    [SerializeField] private float nurseSlideDuration = 0.4f;
    [SerializeField] private float nurseSlideOffset = 300f;
    [SerializeField] private float nurseBubblePopDuration = 0.25f;
    private Tween nurseMoveTween;
    private Tween nurseBubbleTween;
    private Vector2 nurseOriginalPos;

    [System.Serializable]
    public class CharacterPortraitSet
    {
        public Sprite defaultSprite;
        public Sprite happySprite;
        public Sprite sadSprite;
        public Sprite pokerSprite;
        public Sprite sickSprite;
        public Sprite thinkingSprite;
        public Sprite embarrassedSprite;

        public Sprite GetSprite(DialogueLine.ExpressionType expression)
        {
            switch (expression)
            {
                case DialogueLine.ExpressionType.Happy:
                    return happySprite != null ? happySprite : defaultSprite;
                case DialogueLine.ExpressionType.Sad:
                    return sadSprite != null ? sadSprite : defaultSprite;
                case DialogueLine.ExpressionType.Shocked:
                    return pokerSprite != null ? pokerSprite : defaultSprite;
                case DialogueLine.ExpressionType.Angry:
                    return sickSprite != null ? sickSprite : defaultSprite;
                case DialogueLine.ExpressionType.Thinking:
                    return thinkingSprite != null ? thinkingSprite : defaultSprite;
                case DialogueLine.ExpressionType.Embarrassed:
                    return embarrassedSprite != null ? embarrassedSprite : defaultSprite;
                default:
                    return defaultSprite;
            }
        }
    }

    [Header("Alex Portrait")]
    [SerializeField] private Image alexPortraitImage;
    [SerializeField] private CharacterPortraitSet alexPortraits;

    [Header("Alex Internal Portrait")]
    [SerializeField] private Image alexInternalPortraitImage;
    [SerializeField] private CharacterPortraitSet alexInternalPortraits;

    [Header("Boyet Portrait")]
    [SerializeField] private Image boyetPortraitImage;
    [SerializeField] private CharacterPortraitSet boyetPortraits;

    [Header("Lola Mom Portrait")]
    [SerializeField] private Image lolaMomPortraitImage;
    [SerializeField] private CharacterPortraitSet lolaMomPortraits;

    [Header("Nurse Portrait")]
    [SerializeField] private Image nursePortraitImage;
    [SerializeField] private CharacterPortraitSet nursePortraits;

    [Header("Spotlight")]
    [SerializeField] private CanvasGroup[] spotlightHoles;

    [Header("Typewriter")]
    [SerializeField] private float charDelay = 0.03f;

    [Header("Next Indicator")]
    [SerializeField] private GameObject nextIndicator;

    [Header("Scripts")]
    [SerializeField] private TutorialIntro tutorialIntro;

    private int currentIndex = 0;
    private bool isTyping = false;
    private bool canAdvance = false;
    private Coroutine typeCoroutine;
    public bool inputEnabled = true;
    private bool ignoreInputOneFrame = false;

    private Vector2 alexOriginalPos;
    private Vector2 notifOriginalPos;

    private System.Action onFinishedCallback;
    private DialogueLine.SpeakerType? currentSpeaker = null;

    private void Awake()
    {
        if (narratorGroup != null)
        {
            narratorGroup.alpha = 0f;
            narratorGroup.gameObject.SetActive(false);
        }

        if (alexRect != null)
        {
            alexRect.gameObject.SetActive(false);
            alexOriginalPos = alexRect.anchoredPosition;
            alexRect.anchoredPosition = alexOriginalPos + new Vector2(alexSlideOffset, 0f);
        }

        if (alexBubbleGroup != null)
        {
            alexBubbleGroup.alpha = 0f;
            alexBubbleGroup.gameObject.SetActive(false);
        }

        if (alexInternalRect != null)
        {
            alexInternalRect.gameObject.SetActive(false);
            alexInternalOriginalPos = alexInternalRect.anchoredPosition;
            alexInternalRect.anchoredPosition = alexInternalOriginalPos + new Vector2(alexInternalSlideOffset, 0f);
        }

        if (alexInternalBubbleGroup != null)
        {
            alexInternalBubbleGroup.alpha = 0f;
            alexInternalBubbleGroup.gameObject.SetActive(false);
        }

        if (boyetRect != null)
        {
            boyetRect.gameObject.SetActive(false);
            boyetOriginalPos = boyetRect.anchoredPosition;
            boyetRect.anchoredPosition = boyetOriginalPos + new Vector2(boyetSlideOffset, 0f);
        }

        if (boyetBubbleGroup != null)
        {
            boyetBubbleGroup.alpha = 0f;
            boyetBubbleGroup.gameObject.SetActive(false);
        }

        if (nurseRect != null)
        {
            nurseRect.gameObject.SetActive(false);
            nurseOriginalPos = nurseRect.anchoredPosition;
            nurseRect.anchoredPosition = nurseOriginalPos + new Vector2(nurseSlideOffset, 0f);
        }

        if (nurseBubbleGroup != null)
        {
            nurseBubbleGroup.alpha = 0f;
            nurseBubbleGroup.gameObject.SetActive(false);
        }

        if (notifGroup != null)
        {
            notifGroup.alpha = 0f;
            notifGroup.gameObject.SetActive(false);
        }

        if (notifRect != null)
        {
            notifOriginalPos = notifRect.anchoredPosition;
            notifRect.anchoredPosition = notifOriginalPos + new Vector2(notifSlideOffset, 0f);
        }

        if (lolaMomRect != null)
        {
            lolaMomRect.gameObject.SetActive(false);
            lolaMomOriginalPos = lolaMomRect.anchoredPosition;
            lolaMomRect.anchoredPosition = lolaMomOriginalPos + new Vector2(lolaMomSlideOffset, 0f);
        }

        if (lolaMomBubbleGroup != null)
        {
            lolaMomBubbleGroup.alpha = 0f;
            lolaMomBubbleGroup.gameObject.SetActive(false);
        }

        if (spotlightHoles != null)
        {
            foreach (var cg in spotlightHoles)
            {
                if (cg == null) continue;
                cg.alpha = 0f;
                cg.gameObject.SetActive(false);
            }
        }

        if (nextIndicator != null)
            nextIndicator.SetActive(false);
    }

    public void SetDialogueActive(bool value)
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(value);
    }

    public void SetLolaMomActive(bool value)
    {
        if (lolaMomRect != null)
        {
            lolaMomRect.gameObject.SetActive(false);
            lolaMomOriginalPos = lolaMomRect.anchoredPosition;
            lolaMomRect.anchoredPosition = lolaMomOriginalPos + new Vector2(lolaMomSlideOffset, 0f);
        }

        if (lolaMomBubbleGroup != null)
        {
            lolaMomBubbleGroup.alpha = 0f;
            lolaMomBubbleGroup.gameObject.SetActive(false);
        }
    }

    public void BeginDialogue(DialogueData data, System.Action onFinished = null)
    {
        dialogueData = data;
        onFinishedCallback = onFinished;

        currentIndex = 0;
        isTyping = false;
        canAdvance = false;
        waitingForButton = false;
        currentSpeaker = null;

        if (nextIndicator != null) nextIndicator.SetActive(false);

        ignoreInputOneFrame = true;
        ShowCurrentLine();
    }

    private void Update()
    {
        if (!inputEnabled) return;

        if (ignoreInputOneFrame)
        {
            ignoreInputOneFrame = false;
            return;
        }

        // Keep keyboard shortcut
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleAdvanceInput();
        }
    }

    public void OnDialogueNextButtonPressed()
    {
        HandleAdvanceInput();
    }

    private void HandleAdvanceInput()
    {
        if (waitingForButton) return;

        if (isTyping)
        {
            CompleteCurrentTyping();
        }
        else if (canAdvance)
        {
            AdvanceDialogue();
        }
    }

    private void ShowCurrentLine()
    {
        if (dialogueData == null || dialogueData.lines == null) return;

        if (currentIndex >= dialogueData.lines.Length)
        {
            onFinishedCallback?.Invoke();
            return;
        }

        if (hideBubbleCoroutine != null)
        {
            StopCoroutine(hideBubbleCoroutine);
            hideBubbleCoroutine = null;
        }

        lolaMomBubbleHideTween?.Kill();

        DialogueLine line = dialogueData.lines[currentIndex];
        bool speakerChanged = currentSpeaker != line.speaker;

        ApplyExpression(line.speaker, line.expression);

        if (chatLogManager != null)
        {
            chatLogManager.AddLogEntry(line.speaker.ToString(), line.text);
            chatLogManager.gameObject.SetActive(false);
        }

        if (!IsLineValidForBudget(line))
        {
            AdvanceDialogue();
            return;
        }

        canAdvance = false;
        if (nextIndicator != null) nextIndicator.SetActive(false);

        if (line.speaker == DialogueLine.SpeakerType.LolaMom && !line.dependsOnBudget)
        {
            if (GameManager.Instance != null)
            {
                string methodLabel = GameManager.Instance.currentBudgetType == BudgetType.SeventyThirty
                    ? "70/30"
                    : GameManager.Instance.currentBudgetType == BudgetType.FiftyThirtyTwenty
                        ? "50/30/20"
                        : "your chosen method";

                line.text = line.text.Replace("[METHOD]", methodLabel);
            }
        }

        if (line.speaker == DialogueLine.SpeakerType.LolaMom && tutorialIntro != null)
        {
            tutorialIntro.OnLolaStepStarted(line);
            tutorialIntro.OnLolaShowUI(line);
        }

        waitingForButton = line.waitForButton;
        canAdvance = !waitingForButton;

        if (nextIndicator != null)
            nextIndicator.SetActive(!waitingForButton);

        /*UpdateSpotlightForLine(line);*/

        if (speakerChanged)
        {
            HideAllSpeakers();
        }

        switch (line.speaker)
        {
            case DialogueLine.SpeakerType.Narrator:
                ShowNarratorLine(line.text, speakerChanged);
                break;
            case DialogueLine.SpeakerType.Alex:
                ShowAlexLine(line.text, speakerChanged);
                break;
            case DialogueLine.SpeakerType.AlexInternal:
                ShowAlexInternalLine(line.text, speakerChanged);
                break;
            case DialogueLine.SpeakerType.Boyet:
                ShowBoyetLine(line.text, speakerChanged);
                break;
            case DialogueLine.SpeakerType.Notification:
                ShowNotificationLine(line.text, speakerChanged);
                break;
            case DialogueLine.SpeakerType.LolaMom:
                ShowLolaMomLine(line.text, speakerChanged);
                break;
            case DialogueLine.SpeakerType.Nurse:
                ShowNurseLine(line.text, speakerChanged);
                break;
        }

        currentSpeaker = line.speaker;
    }

    private void HideAllSpeakers()
    {
        if (narratorGroup != null) narratorGroup.gameObject.SetActive(false);
        if (alexBubbleGroup != null) alexBubbleGroup.gameObject.SetActive(false);
        if (alexRect != null) alexRect.gameObject.SetActive(false);
        if (alexInternalBubbleGroup != null) alexInternalBubbleGroup.gameObject.SetActive(false);
        if (alexInternalRect != null) alexInternalRect.gameObject.SetActive(false);
        if (boyetBubbleGroup != null) boyetBubbleGroup.gameObject.SetActive(false);
        if (boyetRect != null) boyetRect.gameObject.SetActive(false);
        if (notifGroup != null) notifGroup.gameObject.SetActive(false);
        if (lolaMomBubbleGroup != null) lolaMomBubbleGroup.gameObject.SetActive(false);
        if (lolaMomRect != null) lolaMomRect.gameObject.SetActive(false);
        if (nurseBubbleGroup != null) nurseBubbleGroup.gameObject.SetActive(false);
        if (nurseRect != null) nurseRect.gameObject.SetActive(false);
    }

    private void ShowNarratorLine(string text, bool speakerChanged)
    {
        if (narratorGroup == null || narratorText == null) return;

        narratorGroup.gameObject.SetActive(true);
        narratorText.text = "";

        if (speakerChanged)
        {
            narratorGroup.alpha = 0f;
            narratorGroup.transform.localScale = Vector3.zero;
            narratorGroup.transform.DOScale(1f, narratorPopDuration).SetEase(Ease.OutBack);
            narratorGroup.DOFade(1f, narratorPopDuration);
        }
        else
        {
            narratorGroup.alpha = 1f;
            narratorGroup.transform.localScale = Vector3.one;
        }

        StartTypewriter(narratorText, text);
    }

    private void ShowAlexLine(string text, bool speakerChanged)
    {
        if (alexRect != null)
        {
            alexMoveTween?.Kill();
            alexRect.gameObject.SetActive(true);

            if (speakerChanged)
            {
                alexRect.anchoredPosition = alexOriginalPos + new Vector2(alexSlideOffset, 0f);
                alexMoveTween = alexRect.DOAnchorPos(alexOriginalPos, alexSlideDuration)
                    .SetEase(Ease.OutCubic);
            }
            else
            {
                alexRect.anchoredPosition = alexOriginalPos;
            }
        }

        if (alexBubbleGroup == null || alexText == null) return;

        alexBubbleTween?.Kill();
        alexBubbleGroup.gameObject.SetActive(true);
        alexText.text = "";

        if (speakerChanged)
        {
            alexBubbleGroup.alpha = 0f;
            alexBubbleGroup.transform.localScale = Vector3.zero;
            alexBubbleGroup.transform.localRotation = Quaternion.identity;

            alexBubbleTween = alexBubbleGroup.transform.DOScale(1f, alexBubblePopDuration)
                .SetEase(Ease.OutBack);
            alexBubbleGroup.DOFade(1f, alexBubblePopDuration);
        }
        else
        {
            alexBubbleGroup.alpha = 1f;
            alexBubbleGroup.transform.localScale = Vector3.one;
            alexBubbleGroup.transform.localRotation = Quaternion.identity;
        }

        StartTypewriter(alexText, text);
    }

    private void ShowAlexInternalLine(string text, bool speakerChanged)
    {
        if (alexInternalRect != null)
        {
            alexInternalMoveTween?.Kill();
            alexInternalRect.gameObject.SetActive(true);

            if (speakerChanged)
            {
                alexInternalRect.anchoredPosition = alexInternalOriginalPos + new Vector2(alexInternalSlideOffset, 0f);
                alexInternalMoveTween = alexInternalRect.DOAnchorPos(alexInternalOriginalPos, alexInternalSlideDuration)
                    .SetEase(Ease.OutCubic);
            }
            else
            {
                alexInternalRect.anchoredPosition = alexInternalOriginalPos;
            }
        }

        if (alexInternalBubbleGroup == null || alexInternalText == null) return;

        alexInternalBubbleTween?.Kill();
        alexInternalBubbleGroup.gameObject.SetActive(true);
        alexInternalText.text = "";

        if (speakerChanged)
        {
            alexInternalBubbleGroup.alpha = 0f;
            alexInternalBubbleGroup.transform.localScale = Vector3.zero;
            alexInternalBubbleGroup.transform.localRotation = Quaternion.identity;

            alexInternalBubbleTween = alexInternalBubbleGroup.transform.DOScale(1f, alexInternalBubblePopDuration)
                .SetEase(Ease.OutBack);
            alexInternalBubbleGroup.DOFade(1f, alexInternalBubblePopDuration);
        }
        else
        {
            alexInternalBubbleGroup.alpha = 1f;
            alexInternalBubbleGroup.transform.localScale = Vector3.one;
            alexInternalBubbleGroup.transform.localRotation = Quaternion.identity;
        }

        StartTypewriter(alexInternalText, text);
    }

    private void ShowBoyetLine(string text, bool speakerChanged)
    {
        if (boyetRect != null)
        {
            boyetMoveTween?.Kill();
            boyetRect.gameObject.SetActive(true);

            if (speakerChanged)
            {
                boyetRect.anchoredPosition = boyetOriginalPos + new Vector2(boyetSlideOffset, 0f);
                boyetMoveTween = boyetRect.DOAnchorPos(boyetOriginalPos, boyetSlideDuration)
                    .SetEase(Ease.OutCubic);
            }
            else
            {
                boyetRect.anchoredPosition = boyetOriginalPos;
            }
        }

        if (boyetBubbleGroup == null || boyetText == null) return;

        boyetBubbleTween?.Kill();
        boyetBubbleGroup.gameObject.SetActive(true);
        boyetText.text = "";

        if (speakerChanged)
        {
            boyetBubbleGroup.alpha = 0f;
            boyetBubbleGroup.transform.localScale = Vector3.zero;
            boyetBubbleGroup.transform.localRotation = Quaternion.identity;

            boyetBubbleTween = boyetBubbleGroup.transform.DOScale(1f, boyetBubblePopDuration)
                .SetEase(Ease.OutBack);
            boyetBubbleGroup.DOFade(1f, boyetBubblePopDuration);
        }
        else
        {
            boyetBubbleGroup.alpha = 1f;
            boyetBubbleGroup.transform.localScale = Vector3.one;
            boyetBubbleGroup.transform.localRotation = Quaternion.identity;
        }

        StartTypewriter(boyetText, text);
    }

    private void ShowNurseLine(string text, bool speakerChanged)
    {
        if (nurseRect != null)
        {
            nurseMoveTween?.Kill();
            nurseRect.gameObject.SetActive(true);

            if (speakerChanged)
            {
                nurseRect.anchoredPosition = nurseOriginalPos + new Vector2(nurseSlideOffset, 0f);
                nurseMoveTween = nurseRect.DOAnchorPos(nurseOriginalPos, nurseSlideDuration)
                    .SetEase(Ease.OutCubic);
            }
            else
            {
                nurseRect.anchoredPosition = nurseOriginalPos;
            }
        }

        if (nurseBubbleGroup == null || nurseText == null) return;

        nurseBubbleTween?.Kill();
        nurseBubbleGroup.gameObject.SetActive(true);
        nurseText.text = "";

        if (speakerChanged)
        {
            nurseBubbleGroup.alpha = 0f;
            nurseBubbleGroup.transform.localScale = Vector3.zero;
            nurseBubbleGroup.transform.localRotation = Quaternion.identity;

            nurseBubbleTween = nurseBubbleGroup.transform.DOScale(1f, nurseBubblePopDuration)
                .SetEase(Ease.OutBack);
            nurseBubbleGroup.DOFade(1f, nurseBubblePopDuration);
        }
        else
        {
            nurseBubbleGroup.alpha = 1f;
            nurseBubbleGroup.transform.localScale = Vector3.one;
            nurseBubbleGroup.transform.localRotation = Quaternion.identity;
        }

        StartTypewriter(nurseText, text);
    }

    private void ShowNotificationLine(string text, bool speakerChanged)
    {
        if (notifGroup == null || notifRect == null || notifText == null) return;

        notifGroup.gameObject.SetActive(true);
        notifText.text = "";

        if (speakerChanged)
        {
            notifGroup.alpha = 0f;
            notifRect.anchoredPosition = notifOriginalPos + new Vector2(notifSlideOffset, 0f);
            notifRect.DOAnchorPos(notifOriginalPos, notifSlideDuration).SetEase(Ease.OutCubic);
            notifGroup.DOFade(1f, notifSlideDuration);
        }
        else
        {
            notifGroup.alpha = 1f;
            notifRect.anchoredPosition = notifOriginalPos;
        }

        StartTypewriter(notifText, text);
    }

    private void ShowLolaMomLine(string text, bool speakerChanged)
    {
        if (lolaMomRect != null)
        {
            lolaMomMoveTween?.Kill();
            lolaMomRect.gameObject.SetActive(true);

            if (speakerChanged)
            {
                lolaMomRect.anchoredPosition = lolaMomOriginalPos + new Vector2(lolaMomSlideOffset, 0f);
                lolaMomMoveTween = lolaMomRect.DOAnchorPos(lolaMomOriginalPos, lolaMomSlideDuration)
                    .SetEase(Ease.OutCubic);
            }
            else
            {
                lolaMomRect.anchoredPosition = lolaMomOriginalPos;
            }
        }

        if (lolaMomBubbleGroup == null || lolaMomText == null) return;

        lolaMomBubbleGroup.gameObject.SetActive(true);
        lolaMomText.text = "";

        if (speakerChanged)
        {
            lolaMomBubbleGroup.alpha = 0f;
            lolaMomBubbleGroup.transform.localScale = Vector3.zero;
            lolaMomBubbleGroup.transform.DOScale(1f, lolaMomPopDuration).SetEase(Ease.OutBack);
            lolaMomBubbleGroup.DOFade(1f, lolaMomPopDuration);
        }
        else
        {
            lolaMomBubbleGroup.alpha = 1f;
            lolaMomBubbleGroup.transform.localScale = Vector3.one;
        }

        StartTypewriter(lolaMomText, text);
    }

    /*private void UpdateSpotlightForLine(DialogueLine line)
    {
        if (!line.useSpotlight || spotlightHoles == null)
        {
            foreach (var cg in spotlightHoles)
            {
                if (cg == null) continue;
                cg.DOFade(0f, 0.2f).OnComplete(() => cg.gameObject.SetActive(false));
            }
            return;
        }

        foreach (var cg in spotlightHoles)
        {
            if (cg == null) continue;
            cg.gameObject.SetActive(false);
            cg.alpha = 0f;
        }

        int idx = Mathf.Clamp(line.spotlightIndex, 0, spotlightHoles.Length - 1);
        var targetHole = spotlightHoles[idx];
        if (targetHole != null)
        {
            targetHole.gameObject.SetActive(true);
            targetHole.DOFade(1f, 0.3f);
        }
    }*/

    private bool IsLineValidForBudget(DialogueLine line)
    {
        if (!line.dependsOnBudget) return true;
        if (GameManager.Instance == null) return false;
        return GameManager.Instance.currentBudgetType == line.requiredBudget;
    }

    private void StartTypewriter(TextMeshProUGUI targetText, string fullText)
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        typeCoroutine = StartCoroutine(TypeRoutine(targetText, fullText));
    }

    private IEnumerator TypeRoutine(TextMeshProUGUI targetText, string fullText)
    {
        isTyping = true;
        canAdvance = false;

        targetText.text = "";

        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;

        if (waitingForButton)
        {
            canAdvance = false;
            if (nextIndicator != null) nextIndicator.SetActive(false);

            if (hideBubbleCoroutine != null)
                StopCoroutine(hideBubbleCoroutine);

            hideBubbleCoroutine = StartCoroutine(HideCurrentBubbleAfterDelay(1.5f));
        }
        else
        {
            canAdvance = true;
            if (nextIndicator != null) nextIndicator.SetActive(true);
        }
    }

    private IEnumerator HideCurrentBubbleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (lolaMomBubbleGroup != null)
        {
            lolaMomBubbleHideTween?.Kill();
            lolaMomBubbleHideTween = lolaMomBubbleGroup.DOFade(0f, 0.25f)
                .OnComplete(() =>
                {
                    lolaMomBubbleGroup.gameObject.SetActive(false);

                    if (waitingForButton)
                    {
                        EnableTutorialButtonForCurrentLine();
                    }
                });
        }

        hideBubbleCoroutine = null;
    }

    private void CompleteCurrentTyping()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        if (dialogueData == null || dialogueData.lines == null) return;
        DialogueLine line = dialogueData.lines[currentIndex];

        TextMeshProUGUI target = null;

        switch (line.speaker)
        {
            case DialogueLine.SpeakerType.Narrator:
                target = narratorText;
                break;
            case DialogueLine.SpeakerType.Alex:
                target = alexText;
                break;
            case DialogueLine.SpeakerType.AlexInternal:
                target = alexInternalText;
                break;
            case DialogueLine.SpeakerType.Boyet:
                target = boyetText;
                break;
            case DialogueLine.SpeakerType.Notification:
                target = notifText;
                break;
            case DialogueLine.SpeakerType.LolaMom:
                target = lolaMomText;
                break;
            case DialogueLine.SpeakerType.Nurse:
                target = nurseText;
                break;
        }

        if (target != null)
        {
            target.text = line.text;
        }

        isTyping = false;

        if (waitingForButton)
        {
            canAdvance = false;
            if (nextIndicator != null) nextIndicator.SetActive(false);

            if (hideBubbleCoroutine != null)
                StopCoroutine(hideBubbleCoroutine);

            hideBubbleCoroutine = StartCoroutine(HideCurrentBubbleAfterDelay(1.5f));
        }
        else
        {
            canAdvance = true;
            if (nextIndicator != null) nextIndicator.SetActive(true);
        }
    }

    private void AdvanceDialogue()
    {
        do
        {
            currentIndex++;
            if (dialogueData == null || dialogueData.lines == null) break;
            if (currentIndex >= dialogueData.lines.Length) break;
        }
        while (!IsLineValidForBudget(dialogueData.lines[currentIndex]));

        ShowCurrentLine();
    }

    private void EnableTutorialButtonForCurrentLine()
    {
        if (tutorialIntro == null || dialogueData == null || dialogueData.lines == null) return;
        if (currentIndex < 0 || currentIndex >= dialogueData.lines.Length) return;

        DialogueLine line = dialogueData.lines[currentIndex];
        tutorialIntro.EnableButtonForLolaStep(line.lolaStep);
    }

    public void StartDialogue(DialogueData data)
    {
        dialogueData = data;
        currentIndex = 0;
        isTyping = false;
        canAdvance = false;
        waitingForButton = false;
        currentSpeaker = null;

        if (nextIndicator != null) nextIndicator.SetActive(false);

        ShowCurrentLine();
    }

    public void ContinueFromButton()
    {
        if (!waitingForButton) return;

        DialogueLine line = dialogueData.lines[currentIndex];

        if (line.lolaStep == DialogueLine.LolaStep.ExplainChoices)
        {
            return;
        }

        if (line.lolaStep == DialogueLine.LolaStep.WantsChoices)
        {
            if (systemScreenController == null || !systemScreenController.HasAnyWantSelected())
                return;
        }

        waitingForButton = false;
        canAdvance = false;
        AdvanceDialogue();
    }

    public void ContinueFromAllocationChoices()
    {
        if (!waitingForButton) return;

        DialogueLine line = dialogueData.lines[currentIndex];
        if (line.lolaStep != DialogueLine.LolaStep.ExplainChoices) return;

        waitingForButton = false;
        canAdvance = false;
        AdvanceDialogue();
    }

    private void ApplyExpression(DialogueLine.SpeakerType speaker, DialogueLine.ExpressionType expression)
    {
        switch (speaker)
        {
            case DialogueLine.SpeakerType.Alex:
                if (alexPortraitImage != null && alexPortraits != null)
                    alexPortraitImage.sprite = alexPortraits.GetSprite(expression);
                break;

            case DialogueLine.SpeakerType.AlexInternal:
                if (alexInternalPortraitImage != null && alexInternalPortraits != null)
                    alexInternalPortraitImage.sprite = alexInternalPortraits.GetSprite(expression);
                break;

            case DialogueLine.SpeakerType.Boyet:
                if (boyetPortraitImage != null && boyetPortraits != null)
                    boyetPortraitImage.sprite = boyetPortraits.GetSprite(expression);
                break;

            case DialogueLine.SpeakerType.LolaMom:
                if (lolaMomPortraitImage != null && lolaMomPortraits != null)
                    lolaMomPortraitImage.sprite = lolaMomPortraits.GetSprite(expression);
                break;

            case DialogueLine.SpeakerType.Nurse:
                if (nursePortraitImage != null && nursePortraits != null)
                    nursePortraitImage.sprite = nursePortraits.GetSprite(expression);
                break;
        }
    }
}
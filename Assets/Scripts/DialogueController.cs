using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [Header("Spotlight")]
    //[SerializeField] private CanvasGroup spotlightBgGroup;
    [SerializeField] private CanvasGroup[] spotlightHoles; // one per position

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
    private bool alexHasEntered = false;
    public bool inputEnabled = true;
    private bool ignoreInputOneFrame = false;

    private Vector2 alexOriginalPos;
    private Vector2 notifOriginalPos;

    private System.Action onFinishedCallback;

    private void Awake()
    {
        // Initial visibility / positions
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

    private void Start()
    {
        /*if (dialogueData == null || dialogueData.lines == null || dialogueData.lines.Length == 0)
        {
            Debug.LogWarning("DialogueController: No DialogueData assigned or it is empty.");
            return;
        }

        ShowCurrentLine();*/
    }

    public void SetDialogueActive(bool value)
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(value);
    }
    //for day 1
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

        // Space or left click
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
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

        // Optional text replacement for index 3
        if (line.speaker == DialogueLine.SpeakerType.LolaMom && !line.dependsOnBudget)
        {
            // If this is your “Moving on to your expenses…” line with [METHOD] placeholder
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
        }
        // If this line requires button to continue, block normal advance
        waitingForButton = line.waitForButton;

        canAdvance = !waitingForButton; // only allow Space/click when false
        if (nextIndicator != null) nextIndicator.SetActive(!waitingForButton);

        UpdateSpotlightForLine(line);

        // Hide all groups
        if (narratorGroup != null) narratorGroup.gameObject.SetActive(false);
        if (alexBubbleGroup != null) alexBubbleGroup.gameObject.SetActive(false);
        if (alexRect != null) alexRect.gameObject.SetActive(false);
        if (notifGroup != null) notifGroup.gameObject.SetActive(false);
        if (lolaMomBubbleGroup != null) lolaMomBubbleGroup.gameObject.SetActive(false);
        if (lolaMomRect != null) lolaMomRect.gameObject.SetActive(false);

        // Then show per speaker
        switch (line.speaker)
        {
            case DialogueLine.SpeakerType.Narrator:
                ShowNarratorLine(line.text);
                break;
            case DialogueLine.SpeakerType.Alex:
                ShowAlexLine(line.text);
                break;
            case DialogueLine.SpeakerType.Notification:
                ShowNotificationLine(line.text);
                break;
            case DialogueLine.SpeakerType.LolaMom:
                ShowLolaMomLine(line.text);
                break;
        }
    }

    private void ShowNarratorLine(string text)
    {
        if (narratorGroup == null || narratorText == null) return;

        narratorGroup.gameObject.SetActive(true);
        narratorGroup.alpha = 0f;
        narratorText.text = "";

        narratorGroup.transform.localScale = Vector3.zero;
        narratorGroup.transform.DOScale(1f, narratorPopDuration).SetEase(Ease.OutBack);
        narratorGroup.DOFade(1f, narratorPopDuration);

        StartTypewriter(narratorText, text);
    }

    private void ShowAlexLine(string text)
    {
        if (alexRect != null)
        {
            // Kill previous tween if still running
            alexMoveTween?.Kill();

            alexRect.gameObject.SetActive(true);

            // Reset to offscreen then slide in every time
            alexRect.anchoredPosition = alexOriginalPos + new Vector2(alexSlideOffset, 0f);
            alexMoveTween = alexRect.DOAnchorPos(alexOriginalPos, alexSlideDuration)
                                    .SetEase(Ease.OutCubic);
        }

        if (alexBubbleGroup == null || alexText == null) return;

        alexBubbleTween?.Kill();

        alexBubbleGroup.gameObject.SetActive(true);
        alexBubbleGroup.alpha = 0f;
        alexText.text = "";

        alexBubbleGroup.transform.localScale = Vector3.zero;
        alexBubbleGroup.transform.localRotation = Quaternion.identity;

        alexBubbleTween = alexBubbleGroup.transform.DOScale(1f, alexBubblePopDuration)
                              .SetEase(Ease.OutBack);
        alexBubbleGroup.DOFade(1f, alexBubblePopDuration);

        StartTypewriter(alexText, text);
    }

    private void ShowNotificationLine(string text)
    {
        if (notifGroup == null || notifRect == null || notifText == null) return;

        notifGroup.gameObject.SetActive(true);
        notifGroup.alpha = 0f;
        notifText.text = "";

        notifRect.anchoredPosition = notifOriginalPos + new Vector2(notifSlideOffset, 0f);

        notifRect.DOAnchorPos(notifOriginalPos, notifSlideDuration).SetEase(Ease.OutCubic);
        notifGroup.DOFade(1f, notifSlideDuration);

        StartTypewriter(notifText, text);
    }
    private void ShowLolaMomLine(string text)
    {
        // Character slide-in
        if (lolaMomRect != null)
        {
            lolaMomMoveTween?.Kill();

            lolaMomRect.gameObject.SetActive(true);
            lolaMomRect.anchoredPosition = lolaMomOriginalPos + new Vector2(lolaMomSlideOffset, 0f);
            lolaMomMoveTween = lolaMomRect.DOAnchorPos(lolaMomOriginalPos, lolaMomSlideDuration)
                                          .SetEase(Ease.OutCubic);
        }

        if (lolaMomBubbleGroup == null || lolaMomText == null) return;

        lolaMomBubbleGroup.gameObject.SetActive(true);
        lolaMomBubbleGroup.alpha = 0f;
        lolaMomText.text = "";

        lolaMomBubbleGroup.transform.localScale = Vector3.zero;
        lolaMomBubbleGroup.transform
            .DOScale(1f, lolaMomPopDuration)
            .SetEase(Ease.OutBack);
        lolaMomBubbleGroup
            .DOFade(1f, lolaMomPopDuration);

        StartTypewriter(lolaMomText, text);
    }

    private void UpdateSpotlightForLine(DialogueLine line)
    {
        if (!line.useSpotlight || spotlightHoles == null)
        {
            // Turn all holes off
            foreach (var cg in spotlightHoles)
            {
                if (cg == null) continue;
                cg.DOFade(0f, 0.2f).OnComplete(() => cg.gameObject.SetActive(false));
            }
            return;
        }

        // Disable all holes first
        foreach (var cg in spotlightHoles)
        {
            if (cg == null) continue;
            cg.gameObject.SetActive(false);
            cg.alpha = 0f;
        }

        // Enable the requested hole
        int idx = Mathf.Clamp(line.spotlightIndex, 0, spotlightHoles.Length - 1);
        var targetHole = spotlightHoles[idx];
        if (targetHole != null)
        {
            targetHole.gameObject.SetActive(true);
            targetHole.DOFade(1f, 0.3f);
        }
    }

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
            case DialogueLine.SpeakerType.Notification:
                target = notifText;
                break;
            case DialogueLine.SpeakerType.LolaMom:
                target = lolaMomText;
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

    // Optional: public method so other scripts (like your intro sequence) can start dialogue later
    public void StartDialogue(DialogueData data)
    {
        dialogueData = data;
        currentIndex = 0;
        alexHasEntered = false;
        ShowCurrentLine();
    }

    public void ContinueFromButton()
    {
        if (!waitingForButton) return;

        DialogueLine line = dialogueData.lines[currentIndex];

        if (line.lolaStep == DialogueLine.LolaStep.ExplainChoices)
        {
            if (systemScreenController == null || !systemScreenController.HasRequiredTutorialSelections())
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
}

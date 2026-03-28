using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Dialogue Proper")]
    [SerializeField] private GameObject dialogueRoot;

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

        canAdvance = false;
        if (nextIndicator != null) nextIndicator.SetActive(false);

        DialogueLine line = dialogueData.lines[currentIndex];

        // Hide groups not in use for this line
        if (narratorGroup != null) narratorGroup.gameObject.SetActive(false);
        if (alexBubbleGroup != null) alexBubbleGroup.gameObject.SetActive(false);
        if (alexRect != null) alexRect.gameObject.SetActive(false);
        if (notifGroup != null) notifGroup.gameObject.SetActive(false);
        // Alex image stays (for continuity), but text changes

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
        canAdvance = true;
        if (nextIndicator != null) nextIndicator.SetActive(true);
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
        }

        if (target != null)
        {
            target.text = line.text;
        }

        isTyping = false;
        canAdvance = true;
        if (nextIndicator != null) nextIndicator.SetActive(true);
    }

    private void AdvanceDialogue()
    {
        currentIndex++;
        ShowCurrentLine();
    }

    // Optional: public method so other scripts (like your intro sequence) can start dialogue later
    public void StartDialogue(DialogueData data)
    {
        dialogueData = data;
        currentIndex = 0;
        alexHasEntered = false;
        ShowCurrentLine();
    }
}

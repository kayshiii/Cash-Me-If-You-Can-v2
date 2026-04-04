using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndingManager : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private CanvasGroup backgroundGroup;
    [SerializeField] private float bgFadeInDuration = 0.8f;

    [Header("Dialogue")]
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private DialogueData bestEndingDialogue;
    [SerializeField] private DialogueData okayEndingDialogue;
    [SerializeField] private DialogueData loseEndingDialogue;

    [Header("Ending UI")]
    [SerializeField] private CanvasGroup endingUiGroup;
    [SerializeField] private float endingUiFadeDuration = 0.4f;
    [SerializeField] private TextMeshProUGUI endingTitleText;
    [SerializeField] private TextMeshProUGUI endingDescriptionText;
    [SerializeField] private Button nextButton;
    [SerializeField] private string nextSceneName = "MainMenu"; // or Credits

    private enum EndingType
    {
        Best,
        Okay,
        Lose
    }

    private EndingType chosenEnding;

    private void Awake()
    {
        if (backgroundGroup != null)
        {
            backgroundGroup.alpha = 0f;
            backgroundGroup.gameObject.SetActive(true);
        }

        if (endingUiGroup != null)
        {
            endingUiGroup.alpha = 0f;
            endingUiGroup.gameObject.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(false);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonPressed);
        }
    }

    private void Start()
    {
        if (backgroundGroup != null)
        {
            backgroundGroup.DOFade(1f, bgFadeInDuration).SetEase(Ease.OutCubic);
        }

        chosenEnding = DetermineEnding();
        DialogueData dataToPlay = null;

        switch (chosenEnding)
        {
            case EndingType.Best:
                dataToPlay = bestEndingDialogue;
                break;
            case EndingType.Okay:
                dataToPlay = okayEndingDialogue;
                break;
            case EndingType.Lose:
                dataToPlay = loseEndingDialogue;
                break;
        }

        if (dialogueController != null && dataToPlay != null)
        {
            dialogueController.SetDialogueActive(true);
            dialogueController.inputEnabled = true;
            dialogueController.BeginDialogue(dataToPlay, OnEndingDialogueFinished);
        }
        else
        {
            OnEndingDialogueFinished();
        }
    }

    private EndingType DetermineEnding()
    {
        int totalSavings = 0;
        int happiness = 1;
        int goal = 8500;

        if (GameManager.Instance != null)
        {
            totalSavings = GameManager.Instance.GetCurrentTotalSavings();
            happiness = GameManager.Instance.happiness;
            goal = GameManager.Instance.savingsGoal;
        }

        if (happiness <= 0)
            return EndingType.Lose;

        if (totalSavings >= goal)
            return EndingType.Best;

        return EndingType.Okay;
    }

    private void OnEndingDialogueFinished()
    {
        if (dialogueController != null)
        {
            dialogueController.SetDialogueActive(false);
            dialogueController.inputEnabled = false;
        }

        // Set summary text based on chosenEnding
        if (endingTitleText != null)
        {
            switch (chosenEnding)
            {
                case EndingType.Best:
                    endingTitleText.text = "Best Ending";
                    break;
                case EndingType.Okay:
                    endingTitleText.text = "Okay Ending";
                    break;
                case EndingType.Lose:
                    endingTitleText.text = "Lose Ending";
                    break;
            }
        }

        if (endingDescriptionText != null)
        {
            switch (chosenEnding)
            {
                case EndingType.Best:
                    endingDescriptionText.text =
                        "Alex reached her full Valentine’s savings goal and shared a meaningful, heartfelt date with Boyet.";
                    break;
                case EndingType.Okay:
                    endingDescriptionText.text =
                        "Alex didn’t reach the full goal, but still created something sincere and shared a quiet, genuine moment.";
                    break;
                case EndingType.Lose:
                    endingDescriptionText.text =
                        "Alex couldn’t make the money work out, but learned that her presence and honesty mattered more than the perfect gift.";
                    break;
            }
        }

        if (endingUiGroup != null)
        {
            endingUiGroup.gameObject.SetActive(true);
            endingUiGroup.alpha = 0f;
            endingUiGroup.DOFade(1f, endingUiFadeDuration).SetEase(Ease.OutCubic);
        }

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
        }
    }

    private void OnNextButtonPressed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentDay = 16;
            GameManager.Instance.ResetDayValues(); // no allowance usage anyway
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
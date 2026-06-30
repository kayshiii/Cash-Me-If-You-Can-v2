using UnityEngine;
using UnityEngine.UI;

public class PauseButtonUI : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite unpausedSprite;
    [SerializeField] private Sprite pausedSprite;

    private void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseButtonClicked);

        RefreshSprite();
    }

    private void Update()
    {
        RefreshSprite();
    }

    private void OnPauseButtonClicked()
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.TogglePause();
            RefreshSprite();
        }
    }

    private void RefreshSprite()
    {
        if (PauseManager.Instance == null || buttonImage == null) return;

        buttonImage.sprite = PauseManager.Instance.IsPaused ? pausedSprite : unpausedSprite;
    }
}
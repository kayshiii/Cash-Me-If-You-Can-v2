using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuUI;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Cursor")]
    [SerializeField] private bool manageCursor = true;
    [SerializeField] private bool lockCursorWhenPlaying = true;
    [SerializeField] private bool hideCursorWhenPlaying = true;

    [Header("Audio")]
    [SerializeField] private bool pauseAllAudio = true;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        ApplyCursorState(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;

        if (pauseAllAudio)
            AudioListener.pause = true;

        ApplyCursorState(true);
    }

    public void ResumeGame()
    {
        IsPaused = false;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;

        if (pauseAllAudio)
            AudioListener.pause = false;

        ApplyCursorState(false);
    }

    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Application.Quit();
        Debug.Log("Quit Game");
    }

    private void ApplyCursorState(bool paused)
    {
        if (!manageCursor) return;

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = lockCursorWhenPlaying ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !hideCursorWhenPlaying;
        }
    }
}

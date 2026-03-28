using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuProper : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        Debug.Log("Starting Game...");
        SceneManager.LoadScene("Day 1");
    }

    // Update is called once per frame
    public void QuitGame()
    {
        Application.Quit();
    }
}

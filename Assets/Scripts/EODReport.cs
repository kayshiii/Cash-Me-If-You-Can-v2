using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EODReport : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI savedTodayText;
    [SerializeField] private TextMeshProUGUI specialText;

    private string pendingSpecialMessage = "";
    private bool hasSpecialMessage = false;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void SetSpecialMessage(string message)
    {
        pendingSpecialMessage = message;
        hasSpecialMessage = !string.IsNullOrEmpty(message);

        Debug.Log("[EOD] Special message set: " + message, this);

        if (gameObject.activeInHierarchy)
        {
            RefreshUI();
        }
    }

    public void ClearSpecialMessage()
    {
        pendingSpecialMessage = "";
        hasSpecialMessage = false;

        Debug.Log("[EOD] Special message cleared.", this);

        if (gameObject.activeInHierarchy)
        {
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        if (savedTodayText != null)
        {
            int savedToday = 0;

            if (GameManager.Instance != null)
                savedToday = GameManager.Instance.GetTodaySaved();

            Debug.Log("[EOD] todaySaved = " + savedToday, this);
            savedTodayText.text = "You saved ₱" + savedToday + " today!";
        }

        if (specialText != null)
        {
            if (hasSpecialMessage)
            {
                specialText.gameObject.SetActive(true);
                specialText.text = pendingSpecialMessage;
            }
            else
            {
                specialText.text = "";
                specialText.gameObject.SetActive(false);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EODReport : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI savedTodayText;

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {

        if (savedTodayText == null) return;

        int savedToday = 0;

        Debug.Log("EOD todaySaved = " + savedToday);

        if (GameManager.Instance != null)
            savedToday = GameManager.Instance.GetTodaySaved();

        savedTodayText.text = "You saved ₱" + savedToday + " today!";
    }
}

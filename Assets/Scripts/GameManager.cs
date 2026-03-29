using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BudgetType
{
    None,
    SeventyThirty,   // 70 / 30
    FiftyThirtyTwenty // 50 / 30 / 20
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Values")]
    public int dailyAllowance = 800;          // ₱800 per day [file:4]
    public BudgetType currentBudgetType = BudgetType.None;

    // Optional: track current day, savings, happiness etc. later

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetBudgetType(BudgetType type)
    {
        currentBudgetType = type;
        Debug.Log("Budget chosen: " + type + " | Allowance: " + dailyAllowance);
    }

    // Example helper – how much goes to each category for today
    public void GetBudgetSplit(out int needs, out int wants, out int savings)
    {
        needs = wants = savings = 0;

        switch (currentBudgetType)
        {
            case BudgetType.SeventyThirty:
                // Design doc: 70 = Needs + Wants, 30 = Savings [file:4]
                int combined = Mathf.RoundToInt(dailyAllowance * 0.7f);
                savings = dailyAllowance - combined;
                // You can decide how to split combined internally later
                break;

            case BudgetType.FiftyThirtyTwenty:
                needs = Mathf.RoundToInt(dailyAllowance * 0.5f);
                wants = Mathf.RoundToInt(dailyAllowance * 0.3f);
                savings = dailyAllowance - needs - wants;
                break;
        }
    }
}

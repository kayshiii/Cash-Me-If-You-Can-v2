using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BudgetType
{
    None,
    SeventyThirty,      // 70 / 30
    FiftyThirtyTwenty   // 50 / 30 / 20
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core Values")]
    public int dailyAllowance = 800;
    public BudgetType currentBudgetType = BudgetType.None;

    [Header("Progress")]
    public int currentDay = 1;
    public int happiness = 35;
    public int totalSavings = 0;

    [Header("Savings Goal")]
    public int savingsGoal = 8500;

    [Header("Today State")]
    public bool todayAllocationConfirmed = false;
    public int todaySpent = 0;
    public int todayRemaining = 800;
    public int todaySaved = 0;
    public int todayNeedsSpent = 0;
    public int todayWantsSpent = 0;

    [Header("Today Selected Items")]
    public List<string> draftSelectedItemIds = new List<string>();
    public List<string> confirmedSelectedItemIds = new List<string>();

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
    public void SetTodayCategorySpent(int needs, int wants)
    {
        todayNeedsSpent = Mathf.Max(0, needs);
        todayWantsSpent = Mathf.Max(0, wants);
    }
    public int GetCurrentTotalSavings()
    {
        return totalSavings;
    }

    public void SetCurrentTotalSavings(int value)
    {
        totalSavings = value;

        if (totalSavings < 0)
            totalSavings = 0;

        if (totalSavings > savingsGoal)
            totalSavings = savingsGoal;
    }

    public int GetTodaySaved()
    {
        return todaySaved;
    }

    public void SetTodaySaved(int value)
    {
        todaySaved = Mathf.Max(0, value);
    }

    public void AddSavings(int amount)
    {
        SetCurrentTotalSavings(totalSavings + amount);
    }

    public void SetBudgetType(BudgetType type)
    {
        currentBudgetType = type;
        Debug.Log("Budget chosen: " + type + " | Allowance: " + dailyAllowance);
        ResetDayValues();
    }

    public void GetBudgetSplit(out int needs, out int wants, out int savings)
    {
        needs = wants = savings = 0;

        switch (currentBudgetType)
        {
            case BudgetType.SeventyThirty:
                int combined = Mathf.RoundToInt(dailyAllowance * 0.7f);
                savings = dailyAllowance - combined;
                needs = combined;
                wants = 0;
                break;

            case BudgetType.FiftyThirtyTwenty:
                needs = Mathf.RoundToInt(dailyAllowance * 0.5f);
                wants = Mathf.RoundToInt(dailyAllowance * 0.3f);
                savings = dailyAllowance - needs - wants;
                break;
        }
    }

    public int GetTodaySpendableMoney()
    {
        return dailyAllowance;
    }

    public void SetDraftAllocation(List<string> itemIds, int spent)
    {
        if (todayAllocationConfirmed) return;

        draftSelectedItemIds = new List<string>(itemIds);
        todaySpent = spent;
        todayRemaining = dailyAllowance - todaySpent;

        if (todayRemaining < 0)
            todayRemaining = 0;
    }

    public void ConfirmTodayAllocation(int spent, List<string> itemIds)
    {
        if (todayAllocationConfirmed) return;

        todayAllocationConfirmed = true;
        todaySpent = spent;
        todayRemaining = dailyAllowance - todaySpent;

        if (todayRemaining < 0)
            todayRemaining = 0;

        confirmedSelectedItemIds = new List<string>(itemIds);
    }

    public bool IsTodayConfirmed()
    {
        return todayAllocationConfirmed;
    }

    public void ResetDayValues()
    {
        todayAllocationConfirmed = false;
        todaySpent = 0;
        todayRemaining = dailyAllowance;
        todaySaved = 0;
        draftSelectedItemIds.Clear();
        confirmedSelectedItemIds.Clear();
        todayNeedsSpent = 0;
        todayWantsSpent = 0;
    }

    public void AdvanceDay()
    {
        currentDay++;
        ResetDayValues();
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BudgetType
{
    None,
    SeventyThirty,
    FiftyThirtyTwenty
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

    [Header("Day 3 Choice")]
    public bool day3DecisionMade = false;
    public bool day3PaidNow = false;
    public int day3DebtAmount = 500;
    public bool day3DebtApplied = false;

    [Header("Day 8 Choice")]
    public bool day8DecisionMade = false;
    public bool day8BoughtAntacid = false;
    public int day8AntacidCost = 100;

    [Header("Random Events")]
    public List<string> usedRandomEvents = new List<string>();

    [Header("Choice History")]
    public string lastChosenWantItemId = "";
    public int lastChosenWantDay = -1;

    [Header("Commute History")]
    public string lastCommuteChoiceId = "";
    public int lastCommuteCost = 0;

    [Header("Special Usage History")]
    public List<int> lutoBaonUseDays = new List<int>();
    public List<int> pasabayUseDays = new List<int>();

    public bool WasWantChosenLastDay(string itemId)
    {
        return lastChosenWantItemId == itemId && lastChosenWantDay == currentDay - 1;
    }

    public void RecordWantChoice(string itemId)
    {
        lastChosenWantItemId = itemId;
        lastChosenWantDay = currentDay;
    }

    public void RecordCommuteChoice(string itemId, int cost)
    {
        lastCommuteChoiceId = itemId;
        lastCommuteCost = Mathf.Max(0, cost);

        Debug.Log($"[GameManager] Recorded commute choice: {itemId}, cost: ₱{lastCommuteCost}", this);
    }

    public int CountUsesWithinDays(List<int> useDays, int windowSize)
    {
        int count = 0;
        for (int i = 0; i < useDays.Count; i++)
        {
            if (currentDay - useDays[i] < windowSize)
                count++;
        }
        return count;
    }

    public void RecordLutoBaonUse()
    {
        lutoBaonUseDays.Add(currentDay);
    }

    public void SetDay8Decision(bool boughtAntacid)
    {
        day8DecisionMade = true;
        day8BoughtAntacid = boughtAntacid;

        Debug.Log($"[Day8] Decision stored. Bought antacid = {boughtAntacid}", this);
    }

    public bool HasDay8Decision()
    {
        return day8DecisionMade;
    }

    public bool DidBuyAntacidDay8()
    {
        return day8BoughtAntacid;
    }

    public void SetDay3Decision(bool paidNow)
    {
        day3DecisionMade = true;
        day3PaidNow = paidNow;

        Debug.Log($"[Day3] Decision stored. Paid now = {paidNow}", this);
    }

    public bool HasDay3Decision()
    {
        return day3DecisionMade;
    }

    public bool DidPayDay3DebtNow()
    {
        return day3PaidNow;
    }

    public void ApplyDay3DebtNow(int amount)
    {
        if (day3DebtApplied)
        {
            Debug.Log("[Day3] Debt already applied once. Skipping duplicate apply.", this);
            return;
        }

        day3DebtAmount = amount;
        day3DebtApplied = true;

        int beforeSavings = totalSavings;

        totalSavings -= amount;

        if (totalSavings < 0)
            totalSavings = 0;

        Debug.Log(
            $"[Day3] Debt paid from savings. Debt = ₱{amount}, Savings: ₱{beforeSavings} -> ₱{totalSavings}",
            this
        );
    }

    public float GetHappinessPercent01()
    {
        return Mathf.Clamp01(happiness / 100f);
    }

    public int GetHappinessPercent()
    {
        return Mathf.Clamp(happiness, 0, 100);
    }

    public void AddHappiness(int delta)
    {
        happiness = Mathf.Clamp(happiness + delta, 0, 100);
    }

    public void SpendFromToday(int amount)
    {
        if (amount <= 0) return;

        int beforeSpent = todaySpent;
        int beforeRemaining = todayRemaining;

        todaySpent += amount;
        todayRemaining = Mathf.Max(0, todayRemaining - amount);

        Debug.Log(
            $"[SpendToday] Spent ₱{amount}. todaySpent: {beforeSpent} -> {todaySpent}, todayRemaining: {beforeRemaining} -> {todayRemaining}",
            this
        );
    }

    public void ApplyRandomEventEffects(int savingsDelta, int happinessDelta)
    {
        if (savingsDelta != 0)
        {
            int beforeSavings = totalSavings;
            totalSavings += savingsDelta;
            totalSavings = Mathf.Max(0, totalSavings);

            Debug.Log($"[RandomEvent] Savings: ₱{beforeSavings} -> ₱{totalSavings} (delta {savingsDelta})", this);
        }

        if (happinessDelta != 0)
        {
            int beforeHappiness = happiness;
            happiness = Mathf.Clamp(happiness + happinessDelta, 0, 100);

            Debug.Log($"[RandomEvent] Happiness: {beforeHappiness} -> {happiness} (delta {happinessDelta})", this);
        }
    }

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

    public void AddSavingsBonus(int amount)
    {
        if (amount == 0) return;

        int beforeSavings = totalSavings;

        totalSavings += amount;
        totalSavings = Mathf.Clamp(totalSavings, 0, savingsGoal);

        Debug.Log($"[Savings Bonus] Savings changed by ₱{amount}. Savings: ₱{beforeSavings} -> ₱{totalSavings}", this);
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
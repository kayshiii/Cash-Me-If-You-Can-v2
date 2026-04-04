using UnityEngine;

public enum RandomEventType
{
    Expense,
    Income
}

public enum RandomEventId
{
    None,

    ClassmateEatOut,
    RainCommute,
    TransportStrike,
    BoyetCoffeeTreat,
    SchoolFairMatcha,
    BestFriendDinner,
    MilkteaUpgrade,
    SchoolPlayTicket,

    TitaTutoring,
    UnclePasalubong,
    SellCookies,
    CleanHouse,
    SellCrinkles
}

[System.Serializable]
public class RandomEventChoice
{
    [Header("UI")]
    public string buttonText;
    [TextArea] public string resultText;

    [Header("Effects")]
    public int savingsChange = 0;
    public int happinessChange = 0;
}

[CreateAssetMenu(fileName = "RandomEventData", menuName = "CMIYC/Random Event Data")]
public class RandomEventData : ScriptableObject
{
    [Header("Identity")]
    public RandomEventId eventId = RandomEventId.None;
    public string eventTitle;
    [TextArea] public string description;

    [Header("Type")]
    public RandomEventType eventType = RandomEventType.Expense;

    [Header("Choices")]
    public RandomEventChoice choiceA;
    public RandomEventChoice choiceB;

    [Header("Availability")]
    public int[] eligibleDays;
    public bool oneTimeOnly = true;

    [Header("Requirements")]
    public int minHappiness = 0;
    public int maxHappiness = 100;
    public int minSavings = 0;

    [Header("Optional Weight")]
    [Min(1)] public int weight = 1;

    public bool IsEligible(int currentDay, int currentHappiness, int currentSavings)
    {
        bool dayMatch = false;

        if (eligibleDays != null && eligibleDays.Length > 0)
        {
            for (int i = 0; i < eligibleDays.Length; i++)
            {
                if (eligibleDays[i] == currentDay)
                {
                    dayMatch = true;
                    break;
                }
            }
        }
        else
        {
            dayMatch = true;
        }

        if (!dayMatch) return false;
        if (currentHappiness < minHappiness || currentHappiness > maxHappiness) return false;
        if (currentSavings < minSavings) return false;

        return true;
    }

    private void OnValidate()
    {
        if (maxHappiness < minHappiness)
            maxHappiness = minHappiness;

        if (minSavings < 0)
            minSavings = 0;
    }
}
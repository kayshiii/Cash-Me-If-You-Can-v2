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

[CreateAssetMenu(fileName = "RandomEventData", menuName = "CMIYC/Random Event Data")]
public class RandomEventData : ScriptableObject
{
    [Header("Identity")]
    public RandomEventId eventId = RandomEventId.None;
    public string eventTitle;
    [TextArea] public string description;

    [Header("Type")]
    public RandomEventType eventType = RandomEventType.Expense;

    [Header("Availability")]
    public int[] eligibleDays;
    public bool oneTimeOnly = true;

    [Header("Requirements")]
    public int minHappiness = 0;
    public int maxHappiness = 100;
    public int minSavings = 0;

    [Header("Optional Weight")]
    [Min(1)] public int weight = 1;
}
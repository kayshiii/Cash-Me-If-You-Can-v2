using System.Collections.Generic;
using UnityEngine;

public class RandomEventManager : MonoBehaviour
{
    public static RandomEventManager Instance { get; private set; }

    [Header("Random Event Pool")]
    [SerializeField] private List<RandomEventData> allRandomEvents = new List<RandomEventData>();

    [Header("Event Days")]
    [SerializeField] private List<int> randomEventDays = new List<int> { 6, 9, 13, 14 };

    private RandomEventData currentDayEvent;

    public RandomEventData CurrentDayEvent => currentDayEvent;

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

    public bool IsRandomEventDay(int day)
    {
        return randomEventDays.Contains(day);
    }

    public RandomEventData GenerateRandomEventForDay(int day)
    {
        currentDayEvent = null;

        if (!IsRandomEventDay(day))
            return null;

        List<RandomEventData> candidates = GetEligibleEvents(day);

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"[RandomEventManager] No eligible random events found for Day {day}.", this);
            return null;
        }

        currentDayEvent = GetWeightedRandomEvent(candidates);

        if (currentDayEvent != null)
        {
            Debug.Log($"[RandomEventManager] Selected random event: {currentDayEvent.eventId} on Day {day}", this);
        }

        return currentDayEvent;
    }

    public void ClearCurrentDayEvent()
    {
        currentDayEvent = null;
    }

    private List<RandomEventData> GetEligibleEvents(int day)
    {
        List<RandomEventData> results = new List<RandomEventData>();

        for (int i = 0; i < allRandomEvents.Count; i++)
        {
            RandomEventData data = allRandomEvents[i];

            if (data == null)
                continue;

            if (!IsEligibleForDay(data, day))
                continue;

            results.Add(data);
        }

        return results;
    }

    private bool IsEligibleForDay(RandomEventData data, int day)
    {
        if (data == null)
            return false;

        if (data.eligibleDays == null || data.eligibleDays.Length == 0)
            return false;

        bool validDay = false;
        for (int i = 0; i < data.eligibleDays.Length; i++)
        {
            if (data.eligibleDays[i] == day)
            {
                validDay = true;
                break;
            }
        }

        if (!validDay)
            return false;

        if (GameManager.Instance == null)
            return true;

        if (data.oneTimeOnly && HasBeenUsed(data.eventId))
            return false;

        int happiness = GameManager.Instance.happiness;
        int savings = GameManager.Instance.totalSavings;

        if (happiness < data.minHappiness || happiness > data.maxHappiness)
            return false;

        if (savings < data.minSavings)
            return false;

        return true;
    }

    private RandomEventData GetWeightedRandomEvent(List<RandomEventData> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        int totalWeight = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            totalWeight += Mathf.Max(1, candidates[i].weight);
        }

        int roll = Random.Range(0, totalWeight);

        for (int i = 0; i < candidates.Count; i++)
        {
            int weight = Mathf.Max(1, candidates[i].weight);

            if (roll < weight)
                return candidates[i];

            roll -= weight;
        }

        return candidates[candidates.Count - 1];
    }

    public void MarkEventUsed(RandomEventId eventId)
    {
        if (GameManager.Instance == null || eventId == RandomEventId.None)
            return;

        string key = eventId.ToString();

        if (!GameManager.Instance.usedRandomEvents.Contains(key))
        {
            GameManager.Instance.usedRandomEvents.Add(key);
        }
    }

    public bool HasBeenUsed(RandomEventId eventId)
    {
        if (GameManager.Instance == null || eventId == RandomEventId.None)
            return false;

        return GameManager.Instance.usedRandomEvents.Contains(eventId.ToString());
    }
}
using UnityEngine;

public static class RandomEventResolver
{
    public static RandomEventChoice GetResolvedChoice(RandomEventData data, bool useChoiceA)
    {
        RandomEventChoice resolved = new RandomEventChoice();

        if (data == null)
            return resolved;

        RandomEventChoice source = useChoiceA ? data.choiceA : data.choiceB;

        resolved.buttonText = source.buttonText;
        resolved.resultText = source.resultText;
        resolved.savingsChange = source.savingsChange;
        resolved.happinessChange = source.happinessChange;

        if (GameManager.Instance == null)
            return resolved;

        switch (data.eventId)
        {
            case RandomEventId.SellCookies:
                ResolveSellCookies(useChoiceA, resolved);
                break;

            case RandomEventId.SellCrinkles:
                ResolveSellCrinkles(useChoiceA, resolved);
                break;

            case RandomEventId.RainCommute:
                ResolveRainCommute(useChoiceA, resolved);
                break;

            case RandomEventId.TransportStrike:
                ResolveTransportStrike(useChoiceA, resolved);
                break;

            case RandomEventId.BestFriendDinner:
                ResolveBestFriendDinner(useChoiceA, resolved);
                break;
        }

        return resolved;
    }

    public static void ApplyResolvedChoice(RandomEventData data, bool useChoiceA)
    {
        if (data == null || GameManager.Instance == null)
            return;

        RandomEventChoice resolved = GetResolvedChoice(data, useChoiceA);

        GameManager.Instance.ApplyRandomEventEffects(resolved.savingsChange, resolved.happinessChange);

        if (RandomEventManager.Instance != null)
            RandomEventManager.Instance.MarkEventUsed(data.eventId);

        Debug.Log($"[RandomEventResolver] {data.eventId} | choice {(useChoiceA ? "A" : "B")} | " +
                  $"savingsDelta={resolved.savingsChange}, happinessDelta={resolved.happinessChange}");
    }

    private static void ResolveSellCookies(bool useChoiceA, RandomEventChoice choice)
    {
        if (!useChoiceA)
        {
            choice.savingsChange = 0;
            choice.happinessChange = -3; // -2.5 approx
            return;
        }

        int happiness = GameManager.Instance.GetHappinessPercent();

        if (happiness >= 80)
        {
            choice.savingsChange = 200;   // +500 -300
            choice.happinessChange = 3;
            choice.resultText = "Alex spent ₱300 and earned ₱500 from cookies. Net gain: ₱200.";
        }
        else if (happiness >= 60)
        {
            choice.savingsChange = 100;   // +400 -300
            choice.happinessChange = 2;
            choice.resultText = "Alex spent ₱300 and earned ₱400 from cookies. Net gain: ₱100.";
        }
        else // 40–50
        {
            choice.savingsChange = 50;    // +350 -300
            choice.happinessChange = 1;
            choice.resultText = "Alex spent ₱300 and earned ₱350 from cookies. Net gain: ₱50.";
        }
    }

    private static void ResolveSellCrinkles(bool useChoiceA, RandomEventChoice choice)
    {
        if (!useChoiceA)
        {
            choice.savingsChange = 0;
            choice.happinessChange = -3;
            return;
        }

        int happiness = GameManager.Instance.GetHappinessPercent();

        if (happiness >= 80)
        {
            choice.savingsChange = 200;   // +400 -200
            choice.happinessChange = 3;
            choice.resultText = "Alex spent ₱200 and earned ₱400 from crinkles. Net gain: ₱200.";
        }
        else if (happiness >= 60)
        {
            choice.savingsChange = 100;   // +300 -200
            choice.happinessChange = 2;
            choice.resultText = "Alex spent ₱200 and earned ₱300 from crinkles. Net gain: ₱100.";
        }
        else // 40–50
        {
            choice.savingsChange = 50;    // +250 -200
            choice.happinessChange = 1;
            choice.resultText = "Alex spent ₱200 and earned ₱250 from crinkles. Net gain: ₱50.";
        }
    }

    private static void ResolveBestFriendDinner(bool useChoiceA, RandomEventChoice choice)
    {
        if (!useChoiceA)
        {
            choice.savingsChange = 0;
            choice.happinessChange = -3; // -2.5 approx
            return;
        }

        int commuteCost = GameManager.Instance.lastCommuteCost;
        int totalCost = commuteCost * 2;

        choice.savingsChange = -totalCost;
        choice.happinessChange = 3;
        choice.resultText = $"Alex went to dinner and spent ₱{totalCost} for transportation.";
    }

    private static void ResolveTransportStrike(bool useChoiceA, RandomEventChoice choice)
    {
        if (useChoiceA)
        {
            choice.buttonText = "Take a Grab";
            choice.savingsChange = -450;
            choice.happinessChange = 3;
            choice.resultText = "Alex took a Grab during the transport strike and spent ₱450.";
        }
        else
        {
            choice.buttonText = "Pasabay";

            if (GameManager.Instance.GetHappinessPercent() >= 80)
            {
                choice.savingsChange = 0;
                choice.happinessChange = 0;
                choice.resultText = "Alex got a free pasabay ride.";
            }
            else
            {
                choice.savingsChange = 0;
                choice.happinessChange = 0;
                choice.resultText = "Pasabay was not available because happiness was below 80%.";
            }
        }
    }

    private static void ResolveRainCommute(bool useChoiceA, RandomEventChoice choice)
    {
        string commuteId = GameManager.Instance.lastCommuteChoiceId;
        int commuteCost = GameManager.Instance.lastCommuteCost;

        if (useChoiceA)
        {
            choice.buttonText = "Usual commute";
            choice.savingsChange = -commuteCost;
            choice.happinessChange = -3; // -2.5 approx
            choice.resultText = $"Alex continued her usual commute and spent ₱{commuteCost}.";
        }
        else
        {
            int grabCost = 450;

            if (commuteId == "oober")
                grabCost = 100;
            else if (commuteId == "joykasit")
                grabCost = 300;
            else if (commuteId == "dalawang_sakay")
                grabCost = 400;

            choice.buttonText = "Take a Grab";
            choice.savingsChange = -grabCost;
            choice.happinessChange = 3;
            choice.resultText = $"Because of the rain, Alex took Grab and spent ₱{grabCost}.";
        }
    }
}
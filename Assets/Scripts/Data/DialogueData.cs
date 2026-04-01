using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public enum SpeakerType { Narrator, Alex, Notification, LolaMom }

    public SpeakerType speaker;
    [TextArea] public string text;

    [Header("Optional spotlight")]
    public bool useSpotlight;
    public int spotlightIndex;

    [Header("Optional budget condition")]
    public bool dependsOnBudget;
    public BudgetType requiredBudget;

    [Header("Optional flow control")]
    public bool waitForButton;

    public enum LolaStep
    {
        None,
        ShowStartIntro,
        ExplainLogExpense,
        //ExplainSavings,
        //ExplainDayCounter,
        SystemScreen,
        ExplainChoices,
        WantsSelection,
        WantsChoices,
        Confirm,
        BackToStart,
        PromptSelection,
        ExplainTrack,
        ExitApp
    }
    [Header("Optional Lola step")]
    public LolaStep lolaStep = LolaStep.None;

    [Header("Optional selection requirement")]
    public bool requiresValidAllocation;
}
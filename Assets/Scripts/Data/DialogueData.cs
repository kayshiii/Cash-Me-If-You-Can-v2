using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public enum SpeakerType
    {
        Narrator,
        Alex,
        AlexInternal,
        Boyet,
        Notification,
        LolaMom
    }

    public enum ExpressionType
    {
        Default,
        Happy,
        Sad,
        Shocked,
        Angry,
        Thinking,
        Embarrassed
    }

    [Header("Optional expression")]
    public ExpressionType expression = ExpressionType.Default;

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
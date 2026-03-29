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
}
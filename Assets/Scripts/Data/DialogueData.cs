using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public enum SpeakerType { Narrator, Alex, Notification }

    public SpeakerType speaker;
    [TextArea] public string text;
}
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct DialogueEntry
    {
        public string speakerName;
        [TextArea] public string dialogueText;
    }

    public List<DialogueEntry> dialogues = new List<DialogueEntry>();
}

using System.Collections.Generic;
using UnityEngine;

namespace AIDemo
{
    public enum DialogScene
    {
        Stage_1,
        Beginning,
        Stage_2
    }

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
        public DialogScene dialogScene;
    }
}

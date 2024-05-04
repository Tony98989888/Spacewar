using System.Collections.Generic;
using UnityEngine;

namespace AIDemo
{
    public enum DialogScene
    {
        Prologue,
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

using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace AIDemo
{
    public class DialogManager : MonoBehaviour
    {
        public TMP_Text speakerNameText;
        public TMP_Text dialogueText;
        public GameObject dialoguePanel;

        private DialogueData _currentDialogue;
    
        private Queue<DialogueData.DialogueEntry> dialogueQueue;
        
        [System.Serializable]
        public class OnDialogEndHandler : UnityEvent<DialogueData> { }

        public OnDialogEndHandler onDialogEnd;
    
        void Start()
        {
            dialogueQueue = new Queue<DialogueData.DialogueEntry>();
            dialoguePanel.SetActive(false);
        }
    
        public void StartDialogue(DialogueData dialogueData)
        {
            dialogueQueue.Clear();

            _currentDialogue = dialogueData;
    
            foreach (var dialogueEntry in dialogueData.dialogues)
            {
                dialogueQueue.Enqueue(dialogueEntry);
            }
    
            dialoguePanel.SetActive(true);
            DisplayNextDialogue();
        }
    
        public void DisplayNextDialogue()
        {
            if (dialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }
    
            var dialogueEntry = dialogueQueue.Dequeue();
            speakerNameText.text = dialogueEntry.speakerName;
            dialogueText.text = dialogueEntry.dialogueText;
        }
    
        void Update()
        {
            if (Input.GetMouseButtonDown(0) && dialoguePanel.activeInHierarchy)
            {
                DisplayNextDialogue();
            }
        }
    
        void EndDialogue()
        {
            if (!dialoguePanel.activeInHierarchy)
            {
                return;
            }
            dialoguePanel.SetActive(false);
            onDialogEnd?.Invoke(_currentDialogue);
            _currentDialogue = null;
        }
    }
}



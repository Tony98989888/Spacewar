using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;
    public GameObject dialoguePanel;

    private Queue<DialogueData.DialogueEntry> dialogueQueue;

    void Start()
    {
        dialogueQueue = new Queue<DialogueData.DialogueEntry>();
        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueData dialogueData)
    {
        dialogueQueue.Clear();

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
        dialoguePanel.SetActive(false);
    }
}

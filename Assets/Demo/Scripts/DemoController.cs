using System;
using System.Collections;
using UnityEngine;

namespace AIDemo
{
    public class DemoController : MonoBehaviour
    {
        [SerializeField] private DialogManager dialogManager;


        [SerializeField] private DialogueData prologue;

        private void Start()
        {
            StartCoroutine(ExecuteAfterTime(2, ShowPrologue));
        }

        IEnumerator ExecuteAfterTime(float seconds, System.Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        void ShowPrologue()
        {
            dialogManager.StartDialogue(prologue);
        }
    }
}
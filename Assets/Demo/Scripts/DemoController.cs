using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace AIDemo
{
    public class DemoController : MonoBehaviour
    {
        [SerializeField] private DialogManager dialogManager;

        [SerializeField] private DialogueData prologue;

        [SerializeField] private DialogTrigger wayPointTrigger;

        [SerializeField] private GameState pauseState;
        
        [SerializeField] private GameState gameplayState;

        [SerializeField] private GameStateInputEnabler playerInput;

        private void Start()
        {
            playerInput.gameObject.SetActive(false);
            GameStateManager.Instance.EnterGameState(pauseState);
            StartCoroutine(ExecuteAfterTime(2, ShowPrologue));
            wayPointTrigger.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            dialogManager.onDialogEnd.AddListener(OnDialogEnd);
        }

        private void OnDisable()
        {
            dialogManager.onDialogEnd.RemoveListener(OnDialogEnd);
        }

        private void OnDialogEnd(DialogueData data)
        {
            // When dialogue ends waypoints show up
            if (data.dialogScene == DialogScene.Prologue)
            {
                GameStateManager.Instance.EnterGameState(gameplayState);
                wayPointTrigger.gameObject.SetActive(true);
                playerInput.gameObject.SetActive(true);
            }
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using VSX.UniversalVehicleCombat;

namespace AIDemo
{
    public enum DemoState
    {
        Beginning,
        Stage_1,
        Stage_2,
        Stage_3,
        Stage_4,
        Stage_5
    }

    public class DemoController : MonoBehaviour
    {
        [SerializeField] private DialogManager dialogManager;

        [SerializeField] private DialogueData dialog_1;
        [SerializeField] private DialogueData dialog_2;
        [SerializeField] private DialogueData dialog_3;
        
        [SerializeField] private GameState gameplayState;

        [SerializeField] private GameStateInputEnabler playerInput;

        [SerializeField] private List<WaveController> waves;
        [SerializeField] private List<DemoStageTrigger> triggers;

        private DemoState _demoState;

        private void Start()
        {
            foreach (var trigger in triggers)
            {
                trigger.gameObject.SetActive(false);
            }
            
            foreach (var wave in waves)
            {
                wave.gameObject.SetActive(false);
            }
            
            _demoState = DemoState.Beginning;
            StartCoroutine(ExecuteAfterTime(1, ShowDialog_1));
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
            if (data.dialogScene == DialogScene.Beginning)
            {
                waves[0].gameObject.SetActive(true);
            }
            
            if (data.dialogScene == DialogScene.Stage_1)
            {
                triggers[0].gameObject.SetActive(true);
            }
            
            if (data.dialogScene == DialogScene.Stage_2)
            {
                waves[4].gameObject.SetActive(true);
            }
            
        }

        IEnumerator ExecuteAfterTime(float seconds, System.Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        void ShowDialog_1()
        {
            dialogManager.StartDialogue(dialog_1);
        }

        public void ShowDialog_3()
        {
            dialogManager.StartDialogue(dialog_3);
        }

        public void TriggerNextStage()
        {
            
            if (_demoState == DemoState.Beginning)
            {
                _demoState = DemoState.Stage_1;
                dialogManager.StartDialogue(dialog_2);
                return;
            }
            
            if (_demoState == DemoState.Stage_1)
            {
                _demoState = DemoState.Stage_2;
                triggers[1].gameObject.SetActive(true);
                return;
            }
            
            if (_demoState == DemoState.Stage_2)
            {
                _demoState = DemoState.Stage_3;
                triggers[2].gameObject.SetActive(true);
                return;
            }
            
            if (_demoState == DemoState.Stage_3)
            {
                _demoState = DemoState.Stage_4;
                // Set wrecked ship visible
                triggers[3].gameObject.SetActive(true);
                return;
            }
            
            if (_demoState == DemoState.Stage_4)
            {
                _demoState = DemoState.Stage_5;
                // Flow ends
                return;
            }
            
        }

        public void SpawnWave(int index)
        {
            waves[index].gameObject.SetActive(true);
        }
    }
}
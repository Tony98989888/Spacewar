using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using VSX.UniversalVehicleCombat;

namespace AIDemo
{
    // Stage 0 - Weapons armed, defend yourself
    // Stage 1 - Sure, follow these waypoints.
    
    public enum DemoState
    {
        Stage_0,
        Stage_1,
        Stage_2,
        Stage_3,
        Stage_4,
    }

    public class DemoController : MonoBehaviour
    {
        [SerializeField] private DialogManager dialogManager;

        [FormerlySerializedAs("dialog_1")] [SerializeField] private DialogueData DefendFirstRound;
        [FormerlySerializedAs("dialog_2")] [SerializeField] private DialogueData FollowWaypoints;
        [FormerlySerializedAs("dialog_3")] [SerializeField] private DialogueData DefendCaptain;
        
        [SerializeField] private GameState gameplayState;

        [SerializeField] private GameStateInputEnabler playerInput;

        [SerializeField] private List<WaveController> waves;
        [SerializeField] private List<DemoStageTrigger> triggers;

        [SerializeField] private GameObject wreckedShip;

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
            
            _demoState = DemoState.Stage_0;
            wreckedShip.gameObject.SetActive(false);
            // Demo game flow begins here
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
            if (data.dialogScenario == DialogScenario.DefendFirstRound)
            {
                // After ai says defend yourself spawn 1st round enemies
                waves[0].gameObject.SetActive(true);
            }
            
            if (data.dialogScenario == DialogScenario.FollowWaypoints)
            {
                // When ai says go follow the waypoints, 1st waypoint shows up
                triggers[0].gameObject.SetActive(true);
            }
            
            if (data.dialogScenario == DialogScenario.DefendCaptain)
            {
                waves[3].gameObject.SetActive(true);
            }
            
        }

        IEnumerator ExecuteAfterTime(float seconds, System.Action callback)
        {
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }

        void ShowDialog_1()
        {
            dialogManager.StartDialogue(DefendFirstRound);
        }

        public void OnShowProtectCaptainDialog()
        {
            dialogManager.StartDialogue(DefendCaptain);
        }

        public void TriggerNextStage()
        {
            
            if (_demoState == DemoState.Stage_0)
            {
                _demoState = DemoState.Stage_1;
                dialogManager.StartDialogue(FollowWaypoints);
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
                wreckedShip.gameObject.SetActive(true);
                triggers[2].gameObject.SetActive(true);
                return;
            }
            
            if (_demoState == DemoState.Stage_3)
            {
                _demoState = DemoState.Stage_4;
                // Final stage
                return;
            }
        }

        public void SpawnWave(int index)
        {
            waves[index].gameObject.SetActive(true);
        }
    }
}
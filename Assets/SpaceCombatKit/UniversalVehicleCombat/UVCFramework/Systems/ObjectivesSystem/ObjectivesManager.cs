using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.ObjectivesSystem
{
    /// <summary>
    /// Manages all the objectives that make up the current mission.
    /// </summary>
    public class ObjectivesManager : MonoBehaviour
    {

        [Tooltip("Whether to gather and manage all the objectives in the scene, or only manage the ones added to the list.")]
        [SerializeField]
        protected bool findObjectivesInScene = true;

        [Tooltip("List of components that each control a single objective.")]
        [SerializeField] protected List<ObjectiveController> objectiveControllers = new List<ObjectiveController>();

        [Tooltip("Event called when all the objectives are completed.")]
        public UnityEvent onObjectivesCompleted;

        [Tooltip("The prefab that displays the UI for a single objective.")]
        [SerializeField]
        protected ObjectiveUIController objectiveUIPrefab;

        [Tooltip("The parent transform for the spawned objective UI prefabs.")]
        [SerializeField]
        protected Transform objectiveUIParent;


        // Called in the editor when the component is first added to a gameobject or reset in the inspector
        protected virtual void Reset()
        {
            objectiveUIParent = transform;
        }


        protected virtual void Awake()
        {
            if (findObjectivesInScene)
            {
                ObjectiveController[] objectivesArray = FindObjectsOfType<ObjectiveController>();
                foreach(ObjectiveController objective in objectivesArray)
                {
                    if (objectiveControllers.IndexOf(objective) == -1)
                    {
                        objectiveControllers.Add(objective);
                    }
                }
            }

            foreach (ObjectiveController objectiveController in objectiveControllers)
            {
                ObjectiveUIController objectiveUIController = Instantiate(objectiveUIPrefab, objectiveUIParent);
                objectiveUIController.SetObjective(objectiveController);
                objectiveController.onCompleted.AddListener(CheckCompleted);
            }
        }


        // Check if all the objectives are completed
        protected virtual void CheckCompleted()
        {
            foreach (ObjectiveController objectiveController in objectiveControllers)
            {
                if (!objectiveController.Completed) return;
            }

            onObjectivesCompleted.Invoke();
        }
    }
}

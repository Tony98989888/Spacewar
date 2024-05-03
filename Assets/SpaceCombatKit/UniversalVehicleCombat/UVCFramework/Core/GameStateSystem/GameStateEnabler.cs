using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VSX.UniversalVehicleCombat
{
    /// <summary>
    /// Useful for running functions upon entering or exiting specific game states.
    /// </summary>
    public class GameStateEnabler : MonoBehaviour
    {
        [Tooltip("The game states that trigger the events when entered and exited.")]
        [SerializeField]
        protected List<GameState> compatibleGameStates = new List<GameState>();

        [Tooltip("Event called when one of the compatible game states is entered.")]
        public UnityEvent onCompatibleGameStateEntered;

        [Tooltip("Event called when one of the compatible game states is exited.")]
        public UnityEvent onIncompatibleGameStateEntered;


        protected virtual void Awake()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.onEnteredGameState.AddListener(OnEnteredGameState);
            }
        }


        // Called every time a new game state is entered.
        protected virtual void OnEnteredGameState(GameState gameState)
        {
            if (compatibleGameStates.IndexOf(gameState) != -1)
            {
                onCompatibleGameStateEntered.Invoke();
            }
            else
            {
                onIncompatibleGameStateEntered.Invoke();
            }
        }
    }
}


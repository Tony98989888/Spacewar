using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VSX.UniversalVehicleCombat
{
    public class GameStateCompatibilityChecker : MonoBehaviour
    {

        [Tooltip("The game states that return True for compatibility.")]
        [SerializeField]
        protected List<GameState> compatibleGameStates = new List<GameState>();


        /// <summary>
        /// Check if one of the compatible game states is active.
        /// </summary>
        public bool IsCompatibleGameState
        {
            get
            {
                if (GameStateManager.Instance != null)
                {
                    for(int i = 0; i < compatibleGameStates.Count; ++i)
                    {
                        if (compatibleGameStates[i] == GameStateManager.Instance.CurrentGameState)
                        {
                            return true;
                        }
                    }

                    return false;

                }
                else
                {
                    return true;
                }
            }
        }
    }
}

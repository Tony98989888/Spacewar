using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace VSX.Utilities.UI
{
    /// <summary>
    /// Control a menu (Menu Controller) using Unity's input system.
    /// </summary>
    public class InputSystem_MenuControls : GeneralInput
    {

        [Tooltip("The menu controller to control.")]
        [SerializeField]
        protected MenuController menuController;

        [Tooltip("The game state that should be entered when the menu is opened in the default state.")]
        [SerializeField]
        protected GameState menuGameState;

        protected GeneralInputAsset input;

        protected bool menuToggled = false;



        protected virtual void Reset()
        {
            menuController = GetComponentInChildren<MenuController>();
        }


        protected override void Awake()
        {
            base.Awake();

            input = new GeneralInputAsset();
            input.GeneralControls.Back.performed += ctx => Back();
            input.GeneralControls.Menu.performed += ctx => OpenMenu();

            StartCoroutine(MenuToggleReset());
        }


        protected virtual void OnEnable()
        {
            input.Enable();
        }


        protected virtual void OnDisable()
        {
            input.Disable();
        }


        protected virtual void OpenMenu()
        {
            if (menuToggled) return;

            menuController.OpenMenu();
            if (menuGameState != null)
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.EnterGameState(menuGameState);
                }
            }

            menuToggled = true;
        }


        protected virtual void Back()
        {
            if (menuToggled) return;
            if (menuController.MenuOpen && menuController.CurrentState != null)
            {
                menuController.CurrentState.ExitToParent();
                menuToggled = true;
            }
        }


        // Reset the menu toggled flag. Prevents opening and closing in same frame if the open/close key happens to be the same.
        IEnumerator MenuToggleReset()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                menuToggled = false;
            }
        } 
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

namespace VSX.Utilities.UI
{
    
    [System.Serializable]
    public class MenuOpenInput
    {
        public MenuController menu;
        public CustomInput input;
    }

    public class MenuControls : GeneralInput
    {
        [Header("Menu Input")]

        [Tooltip("Input settings for opening different menus.")]
        [SerializeField]
        protected List<MenuOpenInput> menuOpenInputs = new List<MenuOpenInput>();

        protected override void InputUpdate()
        {
            for(int i = 0; i < menuOpenInputs.Count; ++i)
            {
                if (menuOpenInputs[i].input.Down())
                {
                    menuOpenInputs[i].menu.OpenMenu();
                }
            }
        }
    }
}


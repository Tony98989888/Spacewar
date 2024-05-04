using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VSX.UniversalVehicleCombat;

public class InputSystem_DialogControls : GeneralInput 
{
        protected DialogInputAssets input;

        [SerializeField] private DialogManager dialogManager;

        protected override void Awake()
        {
            base.Awake();

            input = new DialogInputAssets();
            input.Dialog.NextDialog.performed += ctx => { dialogManager.DisplayNextDialogue();};
        }


        protected virtual void OnEnable()
        {
            input.Enable();
        }


        protected virtual void OnDisable()
        {
            input.Disable();
        }
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace VSX.UniversalVehicleCombat.Loadout
{

    /// <summary>
    /// Example input system script for loadout controls.
    /// </summary>
    public class InputSystem_LoadoutControls : GeneralInput
    {

        [SerializeField]
        protected LoadoutUIController loadoutUIController;

        [Tooltip("The loadout camera controller.")]
        [SerializeField]
        protected LoadoutCameraController loadoutCameraController;

        protected LoadoutInputAsset input;

        protected Vector3 lookRotationInputs;
        protected bool lookRotationEngaged = false;


        protected override void Awake()
        {
            base.Awake();
            
            input = new LoadoutInputAsset();

            input.LoadoutControls.Menu.performed += ctx => Menu();

            input.LoadoutControls.Back.performed += ctx => Back();

            input.LoadoutControls.Start.performed += ctx => StartAction();

            input.LoadoutControls.Accept.performed += ctx => Accept();

            input.LoadoutControls.Delete.performed += ctx => Delete();

            input.LoadoutControls.CycleVehicleSelection.performed += ctx => CycleVehicleSelection(ctx.ReadValue<float>());
            
            input.LoadoutControls.CycleModuleSelection.performed += ctx => CycleModuleSelection(ctx.ReadValue<float>());

            input.LoadoutControls.CycleModuleMountSelection.performed += ctx => CycleModuleMountSelection(ctx.ReadValue<float>());


            //input.LoadoutControls.CycleModuleSelectionBack.performed += ctx => CycleModuleSelection(false);


        }


        protected virtual void OnEnable()
        {
            input.Enable();
        }


        protected virtual void OnDisable()
        {
            input.Disable();
        }


        protected virtual void Back()
        {
            if (loadoutUIController.State == LoadoutUIController.UIState.ModuleSelection)
            {
                loadoutUIController.EnterVehicleSelection();
            }
            else
            {
                loadoutUIController.MainMenu();
            }
        }

        public virtual void Menu()
        {
            loadoutUIController.MainMenu();
        }

        public virtual void StartAction()
        {
            loadoutUIController.StartMission(0);
        }

        public virtual void Accept()
        {
            if (loadoutUIController.State == LoadoutUIController.UIState.VehicleSelection)
            {
                loadoutUIController.EnterModuleSelection();
            }
            else
            {
                loadoutUIController.EquipModule();
            }
        }


        protected virtual void CycleVehicleSelection(float val)
        {
            if (loadoutUIController.State == LoadoutUIController.UIState.VehicleSelection) loadoutUIController.CycleVehicleSelection(val > 0 ? true : false);
        }

        protected virtual void CycleModuleSelection(float val)
        {
            if (loadoutUIController.State == LoadoutUIController.UIState.ModuleSelection) loadoutUIController.CycleModuleSelection(val > 0 ? true : false);
        }

        protected virtual void CycleModuleMountSelection(float val)
        {
            if (loadoutUIController.State == LoadoutUIController.UIState.ModuleSelection) loadoutUIController.CycleModuleMountSelection(val > 0 ? true : false);
        }

        protected virtual void Delete()
        {
            if (loadoutUIController.State == LoadoutUIController.UIState.ModuleSelection) loadoutUIController.ClearSelectedModuleMount();
        }

        protected override void InputUpdate()
        {
            base.InputUpdate();

            loadoutCameraController.SetViewRotationInputs(input.LoadoutControls.Look.ReadValue<Vector2>());

        }
    }
}

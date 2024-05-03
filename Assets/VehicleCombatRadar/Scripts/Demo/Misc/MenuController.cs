using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// This script is to toggle the control binding information on and off

namespace VSX.Vehicles { 

	public class MenuController : MonoBehaviour {
	
		public GameObject menu;
		public Text tip;
		public Text numTargetsText;
	
		public PlayerController playerController;
		public SpawnObjects objectSpawn;

		
	
		// Use this for initialization
		void Start () {

			menu.SetActive(false);
			Cursor.visible = false;
			tip.text = "Press 'C' for controls";

		}
		
		// Update is called once per frame
		void Update()
		{

			// Toggle the menu on/off
			if (Input.GetKeyDown(KeyCode.C))
			{
				menu.SetActive(!menu.activeSelf);
				if (menu.activeSelf)
				{
					tip.text = "Press 'C' to close";
					playerController.disabled = true;
					Cursor.visible = true;
				}
				else
				{
					tip.text = "Press 'C' for controls";
					playerController.disabled = false;
					Cursor.visible = false;
				}
			}

			numTargetsText.text = objectSpawn.GetNumObjects().ToString();
		}
	}
}
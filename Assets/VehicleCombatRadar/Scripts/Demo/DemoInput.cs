using UnityEngine;
using System.Collections;
using VSX.Vehicles;

// This script is designed to provide an example of how to access common functionality in this package with player input.

namespace VSX.Vehicles {

	public class DemoInput : MonoBehaviour {
	
		private ITracker tracker;
		bool hasTracker;
	
		HUDRadar3D radar3D;
		bool hasRadar3D;

		HUDVisor visor;
		bool hasVisor;
	
		
		
		void Start()
		{

			// Get the tracker information
			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager == null)
			{
				Debug.LogError("Please add a RadarSceneManager component to the scene, cannot find tracker for DemoInput controls");
			}
			else
			{
				tracker = radarSceneManager.UITracker;
				
				if (tracker == null)
				{
					Debug.LogWarning("No ITracker components found in the scene, please add one. Cannot implement DemoInput radar controls");
				}
				else
				{
					hasTracker = true;
				}
			}
			
			radar3D = GameObject.FindObjectOfType<HUDRadar3D>();
			if (radar3D == null)
			{
				Debug.LogWarning("No HUDRadar3D component found in scene, cannot implement DemoInput 3D radar controls");
			}
			else
			{
				hasRadar3D = true;
			}

			visor = GameObject.FindObjectOfType<HUDVisor>();
			if (visor == null)
			{
				Debug.LogWarning("No HUDVisor component found in scene, cannot implement DemoInput Visor controls");
			}
			else
			{
				hasVisor = true;
			}
			
		}
	
		void Update()
		{
	
			// ****************** Targeting *******************
	
			// New target - hostile
			if (Input.GetKeyDown(KeyCode.E) && hasTracker)
			{
				tracker.GetNewTarget(RadarFunctions.SelectionID.Hostile, RadarFunctions.SelectionMode.Next);
			}
	
			// New target - non-hostile
			if (Input.GetKeyDown(KeyCode.F) && hasTracker)
			{
				tracker.GetNewTarget(RadarFunctions.SelectionID.NonHostile, RadarFunctions.SelectionMode.Next);
			}
	
			// New target - front
			if (Input.GetKeyDown(KeyCode.I) && hasTracker)
			{
				tracker.GetNewTarget(RadarFunctions.SelectionID.Any, RadarFunctions.SelectionMode.Front);
			}
	
			// Nearest target - hostile
			if (Input.GetKeyDown(KeyCode.N) && hasTracker)
			{
				tracker.GetNewTarget(RadarFunctions.SelectionID.Hostile, RadarFunctions.SelectionMode.Nearest);
			}
	
			// Nearest target - non-hostile
			if (Input.GetKeyDown(KeyCode.M) && hasTracker)
			{
				tracker.GetNewTarget(RadarFunctions.SelectionID.NonHostile, RadarFunctions.SelectionMode.Nearest);
			}
	
	
			// *************** Radar3D ***************
	
			// Increase/decrease Radar3D zoom
			if (Input.GetKey(KeyCode.Equals) && hasRadar3D)
			{
				radar3D.IncrementZoom(true);
			}
			else if (Input.GetKey(KeyCode.Minus) && hasRadar3D)
			{
				radar3D.IncrementZoom(false);
			}
	
	
			// *************** Visor *****************
	
			// Toggle arrow mode
			if (Input.GetKeyDown(KeyCode.Alpha1) && hasVisor)
			{
				visor.centerOffscreenArrows = !visor.centerOffscreenArrows;
			}
		}
	}
}
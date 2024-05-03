using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.Vehicles;

// This script is for managing the hologram of the target

namespace VSX.Vehicles {

	public class HUDHologramManager : MonoBehaviour {
	
		public HUDHologramController targetHologram;

		public List<Color> colorByTeam = new List<Color>();
	
		ITracker tracker;
		bool hasTracker;

		ITrackable previousTarget;
		
	
		void Start()
		{

			// Get the tracker information
			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager == null)
			{
				Debug.LogError("Please add a RadarSceneManager component to the scene, cannot link hologram UI to ITracker component");
			}
			else
			{
				tracker = radarSceneManager.UITracker;
				
				if (tracker == null)
				{
					Debug.LogError("No ITracker components found in the scene, please add one. No target hologram information to display");
				}
				else
				{
					hasTracker = true;
				}
			}
		}
	
		void LateUpdate()
		{

			hasTracker = !(tracker == null || tracker.Equals(null));

			if (!hasTracker || !tracker.HasSelectedTarget || !tracker.SelectedTarget.HasHologramMesh)
			{ 
				targetHologram.Disable();
				return;
			}
			
			// If target has changed, update the mesh
			if (tracker.SelectedTarget != previousTarget)
			{
				previousTarget = tracker.SelectedTarget;
				if (!tracker.SelectedTarget.Equals(null))
				{
					
					// Update color
					int colorIndex = (int)(tracker.SelectedTarget.Team);
					targetHologram.SetColor(colorByTeam[colorIndex]);
	
					// Update mesh
					Mesh _mesh = tracker.SelectedTarget.HologramMesh;
					Texture2D _norm = tracker.SelectedTarget.HologramNormal;
					targetHologram.Set(_mesh, _norm);

				}
			}
	
			// Make sure the hologram is enabled
			targetHologram.Enable();
			
			// Update it
			targetHologram.UpdateHologram(tracker.SelectedTarget.GetTransform(), tracker.GetTransform());
		}
	}
}

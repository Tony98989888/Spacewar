using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using VSX.Vehicles;
using VSX.Generic;


// This script controls and updates a 3D radar according to information provided by an ITracker component

namespace VSX.Vehicles{

	// This class is for storing information about how to display widgets for each trackable type in the 
	// RadarFunctions.TrackableType enum. The custom inspector script (HUDRadar3DEditor.cs) controls how 
	// the variables in displayed in the inspector for user modification.

	[System.Serializable]
	public class Radar3D_WidgetSettings {

		public GameObject widgetPrefab;

		public bool fadeUnselectedByDistance;
		
	}

	// This class stores information that will be passed to a widget (component implementing the IRadar3DWidget interface)

	public class Radar3D_WidgetParameters{

		public Vector3 widgetLocalPosition;

		public bool isSelected;

		public Color widgetColor;	

		public float alpha;	

	}


	// This class drives the Radar3D widgets

	public class HUDRadar3D : MonoBehaviour {

		ITracker tracker;
		bool hasTracker;

		[Header("Settings")]

		public float equatorRadius = 0.5f; // Radius of the equator plane

		public float scaleExponent = 1f;	

		public float zoomSpeed = 0.5f;

		public float fadeMaxAlpha;
		public float fadeMinAlpha;
		
		public int maxNewTargetsEachFrame = 1;
		int numTargetsLastFrame;
		int displayedTargetCount;

		float currentZoomValue;

		[Header("Widgets")]

		public List<Color> colorByTeam = new List<Color>();
		
		public List<Radar3D_WidgetSettings> widgetSettingsByType = new List<Radar3D_WidgetSettings>();		// Inspector-defined widget settings by type

		List<List<IRadar3DWidget>> usedWidgetsByType = new List<List<IRadar3DWidget>>();
		List<List<Radar3D_WidgetParameters>> usedWidgetParamsByType = new List<List<Radar3D_WidgetParameters>>();

		List<ObjectPool> widgetPoolsByType = new List<ObjectPool>();

		List<int> usedWidgetIndexByType = new List<int>();

		List<ITrackable> targetsList = new List<ITrackable>();

		

		// Called only in the editor when script is loaded or values are changed
		void OnValidate()
		{	

			// Make sure that the widget object contains a script implementing the IRadar3DWidget interface
			for (int i = 0; i < widgetSettingsByType.Count; ++i)
			{
				if (widgetSettingsByType[i].widgetPrefab != null)
				{
					if (widgetSettingsByType[i].widgetPrefab.GetComponent<IRadar3DWidget>() == null)
					{
						widgetSettingsByType[i].widgetPrefab = null;
						Debug.LogError("Object assigned to Widget Prefab must contain a component implementing the IRadar3DWidget interface");
					}
				}
			}
			
			// Make sure that the assigned exponent is greater than 1
			scaleExponent = Mathf.Max(scaleExponent, 1);
		}
		 
		// Use this for initialization
		void Start () {

			// Get the tracker information
			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager == null)
			{
				Debug.LogError("Please add a RadarSceneManager component to the scene, cannot link 3D radar UI to ITracker component");
			}
			else
			{
				tracker = radarSceneManager.UITracker;
				
				if (tracker == null)
				{
					Debug.LogError ("No ITracker components found in the scene, please add one. No 3D radar information to display.");
				}
				else
				{
					hasTracker = true;
				}
			}
		
			// Make sure that the distance exponent is at least 1
			scaleExponent = Mathf.Max(scaleExponent, 1);

			currentZoomValue = 0f;

			// Prepare the widget pools
			PoolManager poolManager = GameObject.FindObjectOfType<PoolManager>();
			if (poolManager == null)
			{
				Debug.LogError("Unable to find PoolManager component in the scene, please add one");
			}
			else
			{	
				for (int i = 0; i < widgetSettingsByType.Count; ++i){
					if (widgetSettingsByType[i].widgetPrefab == null)
					{
						Debug.LogError("No widget prefab assigned to widget settings instance field in the Radar3D component");
					}
					else
					{
						widgetPoolsByType.Add (poolManager.GetObjectPool(widgetSettingsByType[i].widgetPrefab));
						usedWidgetsByType.Add(new List<IRadar3DWidget>());
						usedWidgetParamsByType.Add(new List<Radar3D_WidgetParameters>());
						usedWidgetIndexByType.Add(-1);
					}
				}
			}

		}

		// Zoom - increment - called by input script
		public void IncrementZoom(bool zoomIn)
		{
			if (zoomIn)
			{ 
				currentZoomValue += zoomSpeed * Time.deltaTime;
			}
			else
			{ 
				currentZoomValue -= zoomSpeed * Time.deltaTime;
			}
			currentZoomValue = Mathf.Clamp(currentZoomValue, 0, 1);
		}

		// Zoom - set - called by input script
		public void SetZoom (float zoomFraction){
	
			currentZoomValue = Mathf.Clamp(zoomFraction, 0, 1);
			
		}	

		// Add a widget from the widget pool
		void AddWidget(int _typeIndex)
		{
			Transform t = widgetPoolsByType[_typeIndex].Get(Vector3.zero, Quaternion.identity).transform;
            t.SetParent(transform);
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
            IRadar3DWidget h = t.GetComponent<IRadar3DWidget>();
			usedWidgetsByType[_typeIndex].Add(h);

			usedWidgetParamsByType[_typeIndex].Add(new Radar3D_WidgetParameters());
		}


		// Update the radar
		void Visualize(ITrackable target)
		{
			
			int typeIndex = (int)(target.TrackableType);
			int teamIndex = (int)(target.Team);
			bool isSelectedTarget = (target == tracker.SelectedTarget);

			// If the target is outside the zoom-adjusted range of this radar, continue
			float radarDisplayRange = (1 - currentZoomValue) * tracker.Range;
			Vector3 targetRelPos = tracker.GetTransform().InverseTransformPoint(target.GetTransform().position);
			float distToTarget = targetRelPos.magnitude;

			if (distToTarget > radarDisplayRange)
			{
				return;
			}

			// Update the last used widget index, and add one if there aren't enough
			usedWidgetIndexByType[typeIndex] += 1;
			if (usedWidgetsByType[typeIndex].Count < usedWidgetIndexByType[typeIndex] + 1)
			{
				AddWidget(typeIndex);
			}
			int thisWidgetIndex = usedWidgetIndexByType[typeIndex];
			
			// Scale the target position to the radar scale 
			float factor = 1 - Mathf.Pow(-(targetRelPos.magnitude / radarDisplayRange) + 1, scaleExponent);
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].widgetLocalPosition = targetRelPos.normalized * (factor * equatorRadius);

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isSelected = isSelectedTarget;

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].widgetColor = colorByTeam[teamIndex];
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = 1f;
			if (!isSelectedTarget && widgetSettingsByType[typeIndex].fadeUnselectedByDistance)
			{
				float fraction = distToTarget / tracker.Range;
				float alphaRange = fadeMaxAlpha - fadeMinAlpha;
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = fadeMaxAlpha - fraction * alphaRange;
			}

			// Set the widget
			usedWidgetsByType[typeIndex][thisWidgetIndex].Set(usedWidgetParamsByType[typeIndex][thisWidgetIndex]);

			// Update the count - used to prevent adding too many widgets per frame
			displayedTargetCount += 1;
			
		}


		void RemoveExcessWidgets(){
			// Remove superfluous widgets
			for (int i = 0; i < usedWidgetsByType.Count; ++i){
				int usedCount = usedWidgetIndexByType[i] + 1;
				if (usedWidgetsByType[i].Count > usedCount)
				{

					int removeCount = usedWidgetsByType[i].Count - usedCount;

					// Disable the widgets
					for (int j = 0; j < usedWidgetsByType[i].Count - usedCount; ++j)
					{
						usedWidgetsByType[i][usedCount + j].Disable();
					}

					usedWidgetsByType[i].RemoveRange(usedCount, removeCount);
					usedWidgetParamsByType[i].RemoveRange(usedCount, removeCount);
				}
			}
		}


		// Update is called once per frame
		void LateUpdate()
		{

			// Reset the used widget index by type
			displayedTargetCount = 0;
			for (int i = 0; i < usedWidgetIndexByType.Count; ++i)
			{
				usedWidgetIndexByType[i] = -1;
			}

			// If no tracker, remove all widgets, otherwise get target info
			hasTracker = !(tracker == null || tracker.Equals(null));
			if (!hasTracker)
			{
				RemoveExcessWidgets();
				return; 	// If no tracker has been found to display target information from, exit 
			}
			else
			{
				// Get targets
				targetsList = tracker.GetTrackedTargets(); // Includes selected target

			}

			// Visualize the targets
			for (int i = 0; i < targetsList.Count; ++i)
			{

				// Don't add a widget for the tracker itself
				if (targetsList[i] == tracker.Trackable() || targetsList[i].Equals(null))
				{
					continue;
				}

				Visualize(targetsList[i]);

				// Don't add more than the specified amount of widgets per frame
				if (displayedTargetCount - numTargetsLastFrame >= maxNewTargetsEachFrame)
				{
					break;
				}

			}

			numTargetsLastFrame = displayedTargetCount;

			RemoveExcessWidgets();

		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.Vehicles;
using VSX.Generic;

// This script provides screen space (visor) target tracking and information

namespace VSX.Vehicles {
	
	// This class is for storing information about how to display widgets for each trackable type in the 
	// RadarFunctions.TrackableType enum. The custom inspector script (HUDVisorEditor.cs) controls how 
	// the variables in displayed in the inspector for user modification.

	[System.Serializable]
	public class Visor_WidgetSettings{

		public GameObject widgetPrefab;
		
		public bool fadeUnselectedByDistance;
		public bool showOffScreenTargets;

		public bool showLabelField;
		
		public bool showValueField;
		
		public bool showBarField;

	}


	// This class stores information that will be passed to a widget (component implementing the IVisorWidget interface)

	public class Visor_WidgetParameters{

		public bool isOnScreen = true;

		public bool isWorldSpace = false;
	
		public Vector3 targetUIPosition = Vector3.zero;
		public Quaternion targetUIRotation;

		public float arrowAngle = 0f;

		public Vector3 leadTargetUIPosition = Vector3.zero;
		public Quaternion leadTargetUIRotation;
		public bool showLeadUI = false;

		public Vector3 cameraPosition;
		public Transform cameraTransform;

		public bool isSelectedTarget = false; 
		
		public RadarFunctions.LockState lockState;
		
		public bool isLocked = false;

		public Color widgetColor = Color.white;

		public float alpha;
		
		public string labelFieldValue;
		public bool showLabelField = true;
		
		public string valueFieldValue;
		public bool showValueField = true;
		
		public float barFieldValue;
		public bool showBarField = true;

		public Vector2 targetMeshSize = Vector2.zero;

		public float scale = 1;
	
		public bool expandingTargetBoxes;

	}


	// This class drives the visor widgets

	public class HUDVisor : MonoBehaviour {
		

		// General settings

		[Tooltip("The UI camera. If none assigned, the main scene camera will be used.")]
		public Camera UICamera;
		Transform UICameraTransform;

		RectTransform canvasRT;
		
		ITracker tracker;
		bool hasTracker = false;

		[Tooltip("The minimum pixel distance between lead target and target box at which the lead target UI will be activated")]
		public float minLeadTargetSeparation;

		[Tooltip("Use the geometrical center of the mesh bounds as the target position.")]
		public bool useMeshBoundsCenter = true;

		[Tooltip("Toggle between offscreen arrows displayed in radial pattern near screen center, or at the screen border.")]
		public bool centerOffscreenArrows = false;

		public float centerOffscreenArrowsRadius = 30;
	
		[Tooltip("The fraction of the viewport which the UI will use to display target information.")]
		public Vector2 UIViewportCoefficients;
		
		[Tooltip("The minimum alpha value of the UI when fading out by distance.")]
		public float fadeMinAlpha = 0.2f;

		[Tooltip("The maximum alpha value of the UI.")]
		public float fadeMaxAlpha = 1f;

		[Tooltip("The maximum number of new targets that can be added each frame, to ensure smooth performance.")]
		public int maxNewTargetsEachFrame = 1;
		int numTargetsLastFrame;

		[Tooltip("Target boxes expand based on the mesh size of the target")]
		public bool expandingTargetBoxes;
		

		RadarFunctions.LockState currentLockUIState;

		List<ObjectPool> widgetPoolsByType = new List<ObjectPool>();


		// World space UI settings

		[Tooltip("Toggle between world-space and screen-space UI.")]
		public bool worldSpaceUI = true;

		[Tooltip("Position the UI at the same world space position as the target.")]
		public bool useTargetWorldPositions = true;

		[Tooltip("The world space distance from the camera at which the UI will be positioned.")]
		public float worldSpaceVisorDistance;

		[Tooltip("A scaling coefficient applied to the widgets to enable them to be easily transferred from screen space mode to world space mode.")]
		public float worldSpaceScaleCoefficient = 1;

		Vector2 m_ViewportSize;
		Vector2 m_ViewportOrigin;
		Vector2 m_ViewportMax;

		Vector2 m_ScreenSize;
		Vector2 m_ScreenOrigin;
		Vector2 m_ScreenMax;

		
		// Widget settings

		public List<Color> colorByTeam = new List<Color>();

		List<List<IVisorWidget>> usedWidgetsByType = new List<List<IVisorWidget>>();

		List<List<Visor_WidgetParameters>> usedWidgetParamsByType = new List<List<Visor_WidgetParameters>>();

		public List<Visor_WidgetSettings> widgetSettingsByType = new List<Visor_WidgetSettings>();

		List<int> usedWidgetIndexByType = new List<int>();
		int displayedTargetCount = 0;		

		List<ITrackable> targetsList;

		Transform assistantTransform;

		// Used for calculating the mesh size for target box expansion
		Vector3[] extentsCorners = new Vector3[8];

		


		// Called only on the editor when script is loaded or values are changed
		void OnValidate()
		{	
		
			// Make sure that the widget object contains a script implementing the IVisorWidget interface
			for (int i = 0; i < widgetSettingsByType.Count; ++i)
			{
				if (widgetSettingsByType[i].widgetPrefab != null)
				{
					if (widgetSettingsByType[i].widgetPrefab.GetComponent<IVisorWidget>() == null)
					{
						Debug.LogError("Visor widget prefab must have a component implementing the IVisorWidget interface");
					}
				}
			}
		}


		
		void Start()
		{
	
			// Get the canvas RectTransform
			Canvas canvas = transform.root.GetComponentInChildren<Canvas>();
			if (canvas == null)
			{
				Debug.LogError("Unable to find canvas in the hierarchy of the HUDVisor component");	
			}
			canvasRT = canvas.GetComponent<RectTransform>();
			
			// Check the camera
			if (UICamera == null)
			{
				UICamera = Camera.main;
			}
			if (UICamera != null)
			{
				UICameraTransform = UICamera.transform;
			}

			// Prepare the widget pools
			PoolManager poolManager = GameObject.FindObjectOfType<PoolManager>();
			if (poolManager == null)
			{
				Debug.LogError("Unable to find PoolManager component in the scene, please add one");
			}
			else
			{
				for (int i = 0; i < widgetSettingsByType.Count; ++i)
				{

					if (widgetSettingsByType[i].widgetPrefab == null)
					{
						Debug.LogError("No widget prefab assigned to widget settings instance field in the HUDVisor component");
					}
					else
					{

						widgetPoolsByType.Add(poolManager.GetObjectPool(widgetSettingsByType[i].widgetPrefab));
						usedWidgetIndexByType.Add(-1);
						usedWidgetsByType.Add(new List<IVisorWidget>());
						usedWidgetParamsByType.Add(new List<Visor_WidgetParameters>());
					}
				}
			}

			// Get the tracker information
			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager == null)
			{
				Debug.LogError("Please add a RadarSceneManager component to the scene, cannot link visor target tracking UI to ITracker component");
			}
			else
			{
				tracker = radarSceneManager.UITracker;
				
				if (tracker == null)
				{
					Debug.LogError ("No ITracker components found in the scene, please add one. No visor target tracking information to display.");
				}
				else
				{
					hasTracker = true;
				}
			}

			assistantTransform = new GameObject().transform;
			assistantTransform.name = "VisorAssistantTransform";
			assistantTransform.SetParent(UICameraTransform);
			assistantTransform.localPosition = Vector3.zero;
			assistantTransform.localRotation = Quaternion.identity;

		}




		// This function returns the screen-space-canvas position of a position in space, as well as whether it is in the camera frame

		public Vector3 GetCanvasPosition(Vector3 targetPos, out bool isOnScreen){
			
			// Necessary to project forward to get correct viewportpoint values
			Vector3 forwardProjectedWorldPos = targetPos;
			bool targetAhead;

			forwardProjectedWorldPos = UICameraTransform.InverseTransformPoint(targetPos);
			targetAhead = forwardProjectedWorldPos.z > 0;
			forwardProjectedWorldPos.z = Mathf.Abs(forwardProjectedWorldPos.z);
			forwardProjectedWorldPos = UICameraTransform.TransformPoint(forwardProjectedWorldPos);

			Vector3 viewPortPoint = UICamera.WorldToViewportPoint(forwardProjectedWorldPos);

			// Check if the target is inside the viewport bounds as defined by the UI viewport coefficients
			isOnScreen = targetAhead && (viewPortPoint.x > m_ViewportOrigin.x && viewPortPoint.x < m_ViewportMax.x) && 
						(viewPortPoint.y > m_ViewportOrigin.y && viewPortPoint.y < m_ViewportMax.y);
			
			// Because canvas origin is at center and screen origin is at left bottom, need to offset by half canvas
			Vector3 halfCanvas = 0.5f * (Vector3)canvasRT.sizeDelta;

			// Get the canvas-space position of the target
			Vector3 screenPos = Vector3.Scale (viewPortPoint, (Vector3)canvasRT.sizeDelta) - halfCanvas;
			
			return screenPos;
	
		}


		
		// This function returns the screen-space-canvas border position of an off-screen target

		Vector2 GetCanvasBorderPosition(Vector3 screenPos, out float arrowAngle)
		{

			// Get the slope of the screen
			float m_ScreenSlope = m_ScreenSize.y/m_ScreenSize.x;

			// Get the origin and the max position of the borders in canvas space
			Vector2 canvasFactor = new Vector2(canvasRT.sizeDelta.x/Screen.width, canvasRT.sizeDelta.y/Screen.height);
			Vector2 m_CanvasOrigin = Vector3.Scale(m_ScreenOrigin, canvasFactor);
			Vector2 m_CanvasMax = Vector3.Scale(m_ScreenMax, canvasFactor);

			// Slope of the target screen position vector relative to the screen center
			float screenPosSlope = screenPos.x != 0 ? screenPos.y/screenPos.x : 0;			// Prevent divide by zero

			// Get the position on the screen border
			Vector2 arrowPos = Vector2.zero;
			if (Mathf.Abs (screenPosSlope) < m_ScreenSlope){ // If the slope is shallower than the screen diagonal, arrow will be on the side of the screen
				
				float factor = ((m_CanvasMax.x - m_CanvasOrigin.x)/2)/Vector3.Magnitude(new Vector3(screenPos.x, 0f, screenPos.z));
				arrowPos = screenPos * factor;

			} else {

				float factor = ((m_CanvasMax.y - m_CanvasOrigin.y)/2)/Vector3.Magnitude(new Vector3(0, screenPos.y, screenPos.z));
				arrowPos = screenPos * factor;	

			}

			// z angle of arrow relative to the screen
			arrowAngle = Mathf.Atan2(arrowPos.y, arrowPos.x) * Mathf.Rad2Deg;	

			return arrowPos;

		}



		// This function calculates the screens-space position of an arrow against a screen-centered radial border pointing to an off-screen target

		Vector2 GetRadialCanvasPosition(Vector2 screenPos, out float arrowAngle)
		{
			Vector2 arrowPos = Vector2.zero;
			
			arrowPos = screenPos.normalized * centerOffscreenArrowsRadius;

			arrowAngle = Mathf.Atan2(arrowPos.y, arrowPos.x) * Mathf.Rad2Deg;	

			return arrowPos;
		}

		

		// Get the world space position and rotation of a widget that is sitting on the to-target axis at a given distance from the camera

		public Vector3 GetWorldPositionOnViewAxis(Vector3 targetPos, float distanceToUI, out Quaternion uiRotation, out bool isOnScreen, bool adjustDistanceToAngle)
		{

			Vector3 toTargetDirection = (targetPos - UICameraTransform.position).normalized;

			uiRotation = Quaternion.LookRotation(toTargetDirection, UICameraTransform.up);

			Vector3 viewPortPos = UICamera.WorldToViewportPoint(targetPos);
			
			// Check if the target is inside the viewport bounds as defined by the UI viewport coefficients
			isOnScreen = viewPortPos.z > 0 && (viewPortPos.x > m_ViewportOrigin.x && viewPortPos.x < m_ViewportMax.x) && (viewPortPos.y > m_ViewportOrigin.y && viewPortPos.y < m_ViewportMax.y);
			
			// Adjust the position by a factor such that the distance to the target along the camera's forward axis is always the same no matter what the angle
			float factor = adjustDistanceToAngle ? 1/Vector3.Dot(UICameraTransform.forward, toTargetDirection) : 1;
			
			return (UICameraTransform.position + (toTargetDirection * distanceToUI * factor));		
			
		}


		// Get the world space position and rotation of an arrow that is pointing at an off-screen target and is up against the border of the screen

		Vector3 GetWorldSpaceScreenBorderPosition(Vector3 worldPos, out Quaternion worldRotation, out float arrowAngle)
		{

			// Necessary to project forward to get correct viewportpoint values
			Vector3 forwardProjectedWorldPos = worldPos;
			forwardProjectedWorldPos = UICameraTransform.InverseTransformPoint(worldPos);
			forwardProjectedWorldPos.z = Mathf.Abs(forwardProjectedWorldPos.z);
			forwardProjectedWorldPos = UICameraTransform.TransformPoint(forwardProjectedWorldPos);
			
			Vector3 viewPortPos = UICamera.WorldToViewportPoint(forwardProjectedWorldPos);
			
			float m_ScreenSlope = m_ScreenSize.y/m_ScreenSize.x;

			Vector3 screenPos = Vector3.Scale(viewPortPos, new Vector3(Screen.width, Screen.height, 0f));
			Vector3 centeredScreenPos = screenPos - (0.5f * new Vector3(Screen.width, Screen.height, 0f));
			float screenPosSlope = centeredScreenPos.y/centeredScreenPos.x;
			
			Vector3 arrowPos = Vector3.zero;

			if (Mathf.Abs (screenPosSlope) < m_ScreenSlope){ // If the slope is shallower than the screen diagonal, arrow will be on the side of the screen

				float factor = ((m_ScreenMax.x - m_ScreenOrigin.x)/2)/Vector3.Magnitude(new Vector3(centeredScreenPos.x, 0f, centeredScreenPos.z));
				arrowPos = centeredScreenPos * factor;

			} else {

				float factor = ((m_ScreenMax.y - m_ScreenOrigin.y)/2)/Vector3.Magnitude(new Vector3(0, centeredScreenPos.y, centeredScreenPos.z));
				arrowPos = centeredScreenPos * factor;

			}
			
			// Calculate the local arrow angle
			arrowAngle = Mathf.Atan2(arrowPos.y, arrowPos.x) * Mathf.Rad2Deg;				

			// Convert back to correct screen space (relative to bottom left corner, not center)
			arrowPos += new Vector3(Screen.width, Screen.height, 0) * 0.5f;

			// Convert to world coordinates
			arrowPos.z = worldSpaceVisorDistance;
			
			arrowPos = UICamera.ScreenToWorldPoint(arrowPos);
			
			// project along the to-target axis by the UI distance from camera value
			arrowPos = UICameraTransform.position + (arrowPos - UICameraTransform.position).normalized * worldSpaceVisorDistance;
			
			// return the correct rotation
			Vector3 toTargetDirection = (arrowPos - UICameraTransform.position).normalized;
			worldRotation = Quaternion.LookRotation(toTargetDirection, UICameraTransform.up);
			
			// return the position
			return arrowPos;

		}



		// Get the world space position and local rotation (relative to the camera-oriented parent widget object) of an arrow against 
		// a radial border, pointing at an offscreen target

		Vector3 GetWorldSpaceArrowRadialPosition(Vector3 worldPos, out Quaternion worldRotation, out float arrowAngle)
		{

			// Get the relative position and flatten it on the local z axis, this will give the correct relative xy direction
			Vector3 result = UICameraTransform.InverseTransformPoint(worldPos);
			result.z = 0;

			// Arrow angle relative to screen
			arrowAngle = Mathf.Atan2(result.y, result.x) * Mathf.Rad2Deg;	

			// Scale the relative direction and transfer it back to world space
			result = Vector3.forward + result.normalized * centerOffscreenArrowsRadius;
			Vector3 direction = UICameraTransform.TransformDirection(result.normalized);
			result = UICameraTransform.position + direction * worldSpaceVisorDistance;

			// return the correct rotation
			worldRotation = Quaternion.LookRotation(direction, UICameraTransform.up);

			return result;
			
		}



		// Toggle arrow center mode with script

		public void SetOffscreenMode(bool centerArrows)
		{
			centerOffscreenArrows = centerArrows;
		}



		// Get the screen-space distance between two points
		float GetScreenDistance(Vector3 pos1, Vector3 pos2)
		{

			Vector3 screenPos1 = UICameraTransform.InverseTransformPoint(pos1);
			screenPos1.z = Mathf.Abs(screenPos1.z);
			screenPos1 = UICamera.WorldToScreenPoint(UICameraTransform.TransformPoint(screenPos1));
			screenPos1.z = 0;
			
			Vector3 screenPos2 = UICameraTransform.InverseTransformPoint(pos2);
			screenPos2.z = Mathf.Abs(screenPos2.z);
			screenPos2 = UICamera.WorldToScreenPoint(UICameraTransform.TransformPoint(screenPos2));
			screenPos2.z = 0;

			return (Vector3.Magnitude(screenPos1 - screenPos2));

		}

		
		// Get the screen-space size of a mesh displayed by a mesh renderer, using its bounding box

		Vector2 GetSizeOnScreen(Mesh _mesh, Transform _transform)
		{

			// Get the positions of all of the corners of the bounding box
			Vector3 extents = _mesh.bounds.extents;
			extentsCorners[0] = extents;
			extentsCorners[1] = new Vector3(-extents.x, extents.y, extents.z);
			extentsCorners[2] = new Vector3(extents.x, -extents.y, extents.z);
			extentsCorners[3] = new Vector3(extents.x, extents.y, -extents.z);
			extentsCorners[4] = new Vector3(-extents.x, -extents.y, -extents.z);
			extentsCorners[5] = new Vector3(-extents.x, -extents.y, extents.z);
			extentsCorners[6] = new Vector3(-extents.x, extents.y, -extents.z);
			extentsCorners[7] = new Vector3(extents.x, -extents.y, -extents.z);	


			// Get the screen position of all of the box corners
			for (int i = 0; i < 8; ++i)
			{	
				bool tmp;
				extentsCorners[i] = GetCanvasPosition(_transform.TransformPoint(_mesh.bounds.center + extentsCorners[i]), out tmp);
			}

			// Find the minimum and maximum bounding box corners in screen space
			Vector3 min = extentsCorners[0];
			Vector3 max = extentsCorners[0];
			for (int i = 1; i < 8; ++i)
			{
				min = Vector3.Min(extentsCorners[i], min);
				max = Vector3.Max(extentsCorners[i], max);
			}
			
			return (new Vector3(max.x - min.x, max.y - min.y, 0f));
		}



		// Get the world-space size (from the cameras perspective) of a mesh displayed by a mesh renderer on the screen

		Vector2 GetSizeOnVisor(Mesh _mesh, Transform _transform)
		{

			// Get the positions of all of the corners of the bounding box
			Vector3 extents = _mesh.bounds.extents;
			extentsCorners[0] = extents;
			extentsCorners[1] = new Vector3(-extents.x, extents.y, extents.z);
			extentsCorners[2] = new Vector3(extents.x, -extents.y, extents.z);
			extentsCorners[3] = new Vector3(extents.x, extents.y, -extents.z);
			extentsCorners[4] = new Vector3(-extents.x, -extents.y, -extents.z);
			extentsCorners[5] = new Vector3(-extents.x, -extents.y, extents.z);
			extentsCorners[6] = new Vector3(-extents.x, extents.y, -extents.z);
			extentsCorners[7] = new Vector3(extents.x, -extents.y, -extents.z);	

			for (int i = 0; i < 8; ++i)
			{	

				bool tmp;
				Quaternion tempQuat; // not used
				float dist = useTargetWorldPositions ? Vector3.Distance(UICameraTransform.position, _transform.position) : worldSpaceVisorDistance;
				extentsCorners[i] = GetWorldPositionOnViewAxis(_transform.TransformPoint(_mesh.bounds.center + extentsCorners[i]), dist, out tempQuat, out tmp, true);
					
			}

			assistantTransform.LookAt(_transform.position, UICameraTransform.up);

			Vector3 min = assistantTransform.InverseTransformPoint(extentsCorners[0]);
			Vector3 max = min;
			
			for (int i = 1; i < 8; ++i)
			{
				Vector3 tmp = assistantTransform.InverseTransformPoint(extentsCorners[i]);
				
				min = Vector3.Min(tmp, min);
				max = Vector3.Max(tmp, max);
				
			}
			
			return (new Vector3(max.x - min.x, max.y - min.y, 0f));

		}
	

		
		// Get another target widget from the pool

		void AddWidget(int _typeIndex){

			Transform t = widgetPoolsByType[_typeIndex].Get(Vector3.zero, Quaternion.identity).transform;
			usedWidgetsByType[_typeIndex].Add(t.GetComponent<IVisorWidget>());
			usedWidgetParamsByType[_typeIndex].Add(new Visor_WidgetParameters());
			t.SetParent(transform);
			t.localScale = new Vector3 (1f, 1f, 1f);

		}



		// Track a target

		void Visualize(ITrackable target)
		{
			
			int typeIndex = (int)(target.TrackableType);
			int teamIndex = (int)(target.Team);
			bool isSelectedTarget = (target == tracker.SelectedTarget);
			
			float distanceToTarget = Vector3.Distance(tracker.GetTransform().position, target.GetTransform().position);
			float cameraDistToTarget = Vector3.Distance(UICameraTransform.position, target.GetTransform().position);
			
			bool isOnScreen = true;
			float arrowAngle = 0f;


			// Get the target position

			Vector3 targetPos;
			if (useMeshBoundsCenter && target.HasBodyMesh)
			{
				targetPos = target.GetTransform().TransformPoint(target.BodyMesh.bounds.center);
			}
			else
			{
				targetPos = target.GetTransform().position;
			}


			// First determine if target is onscreen so that you can skip if visor is set not to show offscreen targets

			Vector3 targetUIPos;
			Quaternion targetUIRotation = Quaternion.identity;

			if (worldSpaceUI)
			{
				float calcDist = useTargetWorldPositions ? cameraDistToTarget : worldSpaceVisorDistance;
				targetUIPos = GetWorldPositionOnViewAxis(targetPos, calcDist, out targetUIRotation, out isOnScreen, false);
			}
			else
			{
				targetUIPos = GetCanvasPosition(targetPos, out isOnScreen);
			}

			// If not on screen and visor is not set to show offscreen targets, skip
			if (!isOnScreen && !widgetSettingsByType[typeIndex].showOffScreenTargets)
				return;
			

			// Update the last used target box, and add one if there aren't enough
			usedWidgetIndexByType[typeIndex] += 1;
			if (usedWidgetsByType[typeIndex].Count < usedWidgetIndexByType[typeIndex] + 1)
			{
				AddWidget(typeIndex);
			}
			int thisWidgetIndex = usedWidgetIndexByType[typeIndex];


			// Tell the widget if is world space or screen space
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isWorldSpace = worldSpaceUI;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].expandingTargetBoxes = expandingTargetBoxes;

			// Pass the camera data to the widget
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].cameraPosition = UICameraTransform.position;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].cameraTransform = UICameraTransform;

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isOnScreen = isOnScreen;


			if (!isOnScreen)
			{
				
				if (worldSpaceUI)
				{
					if (centerOffscreenArrows)
					{
						targetUIPos = GetWorldSpaceArrowRadialPosition(targetPos, out targetUIRotation, out arrowAngle);
					}
					else
					{
						targetUIPos = GetWorldSpaceScreenBorderPosition(targetPos, out targetUIRotation, out arrowAngle);
					}
				}
				else
				{
					if (centerOffscreenArrows)
						targetUIPos = GetRadialCanvasPosition(targetUIPos, out arrowAngle);
					else
						targetUIPos = GetCanvasBorderPosition(targetUIPos, out arrowAngle);
				}

				usedWidgetParamsByType[typeIndex][thisWidgetIndex].arrowAngle = arrowAngle;
			}


			// Pass position and rotation for the widget

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetUIPosition = targetUIPos;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetUIRotation = targetUIRotation;


			// Expanding target box

			if (worldSpaceUI)
			{
				if (target.HasBodyMesh)
				{
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = GetSizeOnVisor(target.BodyMesh, target.GetTransform());
				}
				else
				{
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = Vector2.zero;
				}
				float scale = Vector3.Distance(targetUIPos, UICameraTransform.position) * worldSpaceScaleCoefficient;

				scale = Mathf.Max(scale, 0.00001f);	// Prevent errors when setting UI parameters

				usedWidgetParamsByType[typeIndex][thisWidgetIndex].scale = scale;
			}
			else
			{
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].targetMeshSize = GetSizeOnScreen(target.BodyMesh, target.GetTransform());	
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].scale = 1;
			}
			

			// Lead target and locking information

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].isSelectedTarget = isSelectedTarget;
			if (isSelectedTarget)
			{
				Quaternion leadTargetUIRotation = Quaternion.identity;
					
				if (worldSpaceUI)
				{

					bool isOnScreen2; // not used

					float calcDist = useTargetWorldPositions ? cameraDistToTarget : worldSpaceVisorDistance;

					Vector3 leadTargetUIPos = GetWorldPositionOnViewAxis(tracker.SelectedTargetLeadPosition, calcDist, out leadTargetUIRotation, out isOnScreen2, false);
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].leadTargetUIPosition = leadTargetUIPos;
						
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].showLeadUI = GetScreenDistance(targetUIPos, leadTargetUIPos) > minLeadTargetSeparation;

				}
				else
				{
					// Since the lead target is a child, subtract the target position from the calculated position
					Vector3 leadTargetUIPos = GetCanvasPosition(tracker.SelectedTargetLeadPosition, out isOnScreen) - targetUIPos;
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].leadTargetUIPosition = leadTargetUIPos;
					usedWidgetParamsByType[typeIndex][thisWidgetIndex].showLeadUI = new Vector3(leadTargetUIPos.x, leadTargetUIPos.y, 0f).magnitude > minLeadTargetSeparation;
				}

				usedWidgetParamsByType[typeIndex][thisWidgetIndex].lockState = tracker.CurrentLockState;
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].leadTargetUIRotation = leadTargetUIRotation;
			} 


			// Color information

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].widgetColor = colorByTeam[teamIndex];
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = 1f;
			if (!isSelectedTarget && widgetSettingsByType[typeIndex].fadeUnselectedByDistance)
			{
				float fraction = distanceToTarget / tracker.Range;
				float alphaRange = fadeMaxAlpha - fadeMinAlpha;
				usedWidgetParamsByType[typeIndex][thisWidgetIndex].alpha = fadeMaxAlpha - fraction * alphaRange;
			}
			
			
			// Widget label field

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].labelFieldValue = target.TrackableName;
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].showLabelField = widgetSettingsByType[typeIndex].showLabelField;


			// Widget value field

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].valueFieldValue = RadarDistanceLookup.Lookup(distanceToTarget);
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].showValueField = widgetSettingsByType[typeIndex].showValueField;


			// Widget bar field

			usedWidgetParamsByType[typeIndex][thisWidgetIndex].barFieldValue = target.GetCurrentHealth()/target.GetMaxHealth();
			usedWidgetParamsByType[typeIndex][thisWidgetIndex].showBarField = widgetSettingsByType[typeIndex].showBarField;

		
			// Enable the widget and pass the data
			
			usedWidgetsByType[typeIndex][thisWidgetIndex].Enable();
			usedWidgetsByType[typeIndex][thisWidgetIndex].Set(usedWidgetParamsByType[typeIndex][thisWidgetIndex]);

			displayedTargetCount += 1;		
			
		}


		// Return unused widgets to the pool

		void RemoveExcessWidgets()
		{
			
			// Remove superfluous widgets
			for (int i = 0; i < usedWidgetsByType.Count; ++i){
				int usedCount = usedWidgetIndexByType[i] + 1;
				if (usedWidgetsByType[i].Count > usedCount)
				{
					
					int removeAmount = usedWidgetsByType[i].Count - usedCount;
					
					// Disable the widgets
					for (int j = 0; j < removeAmount; ++j)
					{
						usedWidgetsByType[i][usedCount + j].Disable();
					}

					
					usedWidgetsByType[i].RemoveRange(usedCount, removeAmount);
					usedWidgetParamsByType[i].RemoveRange(usedCount, removeAmount);
					
				}
			}
		}


		// Do UI just before render

		void LateUpdate()
		{


			// Calculate the UIViewportCoefficient-scaled viewport origin/max and screen origin/max once per frame, 
			// so it is not being done for every widget

			Vector2 viewPortSize = new Vector2(1,1);
			Vector2 screenSize = new Vector2 (Screen.width, Screen.height);

			m_ViewportSize = Vector2.Scale(UIViewportCoefficients, viewPortSize);
			m_ViewportOrigin = (viewPortSize - m_ViewportSize) * 0.5f;
			m_ViewportMax = viewPortSize - (viewPortSize - m_ViewportSize) * 0.5f;

			m_ScreenSize = Vector2.Scale(m_ViewportSize, screenSize);
			m_ScreenOrigin = Vector2.Scale(m_ViewportOrigin, screenSize);
			m_ScreenMax = Vector2.Scale(m_ViewportMax, screenSize);
	

			// Clamp the world space visor distance to a non-zero value

			worldSpaceVisorDistance = Mathf.Max(worldSpaceVisorDistance, 0.0001f);


			// Reset the per-frame target-count and used widget index info

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
			

			// Visualise targets

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
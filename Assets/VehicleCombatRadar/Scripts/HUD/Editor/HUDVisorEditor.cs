﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.Vehicles;

// This script is to modify the inspector of the HUDRadar3D so that the user can set different parameters
// for displaying targets of different types
namespace VSX.Vehicles {

	[CustomEditor(typeof(HUDVisor))]
	public class HUDVisorEditor : Editor {
	
		HUDVisor script;
		
		// General settings

		SerializedProperty hudCameraProperty;
		SerializedProperty uiViewportCoefficientsProperty;
		SerializedProperty useMeshBoundsCenterProperty;
		SerializedProperty minLeadTargetSeparationProperty;
		SerializedProperty centerOffscreenArrowsProperty;
		SerializedProperty centerOffscreenArrowsRadiusProperty;
		SerializedProperty fadeMinAlphaProperty;
		SerializedProperty fadeMaxAlphaProperty;
		SerializedProperty maxNewTargetsEachFrameProperty;
		SerializedProperty expandingTargetBoxesProperty;

		// World space settings

		SerializedProperty toggleWorldSpaceUIProperty;
		SerializedProperty useTargetWorldPositionsProperty;
		SerializedProperty worldSpaceVisorDistanceProperty;
		SerializedProperty worldSpaceScaleCoefficientProperty;


		void OnEnable()
		{

			script = (HUDVisor)target;
	
			
			// General settings

			hudCameraProperty = serializedObject.FindProperty("UICamera");
			uiViewportCoefficientsProperty = serializedObject.FindProperty("UIViewportCoefficients");
			useMeshBoundsCenterProperty = serializedObject.FindProperty("useMeshBoundsCenter");
			minLeadTargetSeparationProperty = serializedObject.FindProperty("minLeadTargetSeparation");
			centerOffscreenArrowsProperty = serializedObject.FindProperty("centerOffscreenArrows");
			centerOffscreenArrowsRadiusProperty = serializedObject.FindProperty("centerOffscreenArrowsRadius");
			fadeMinAlphaProperty = serializedObject.FindProperty("fadeMinAlpha");
			fadeMaxAlphaProperty = serializedObject.FindProperty("fadeMaxAlpha");
			maxNewTargetsEachFrameProperty = serializedObject.FindProperty("maxNewTargetsEachFrame");
			expandingTargetBoxesProperty = serializedObject.FindProperty("expandingTargetBoxes");


			// World space settings
			
			toggleWorldSpaceUIProperty = serializedObject.FindProperty("worldSpaceUI");
			useTargetWorldPositionsProperty = serializedObject.FindProperty("useTargetWorldPositions");
			worldSpaceVisorDistanceProperty = serializedObject.FindProperty("worldSpaceVisorDistance");
			worldSpaceScaleCoefficientProperty = serializedObject.FindProperty("worldSpaceScaleCoefficient");
			
		}

 		public override void OnInspectorGUI()
		{
	
			// Setup
			serializedObject.Update();

			string[] typeNames = Enum.GetNames(typeof(RadarFunctions.TrackableType));
			string[] teamNames = Enum.GetNames(typeof(RadarFunctions.Team));

			// Resize lists in the layout (not Repaint!) phase
			if (Event.current.type == EventType.Layout)
			{
				RadarFunctions.ResizeList(script.widgetSettingsByType, typeNames.Length);
				RadarFunctions.ResizeList(script.colorByTeam, teamNames.Length);
				serializedObject.ApplyModifiedProperties();
				serializedObject.Update();
			}


			// General settings

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(hudCameraProperty);

			EditorGUILayout.PropertyField(uiViewportCoefficientsProperty);

			EditorGUILayout.PropertyField(useMeshBoundsCenterProperty);

			EditorGUILayout.PropertyField(minLeadTargetSeparationProperty);
	
			EditorGUILayout.PropertyField(centerOffscreenArrowsProperty);
			
			EditorGUILayout.PropertyField(centerOffscreenArrowsRadiusProperty);
			
			EditorGUILayout.PropertyField(fadeMinAlphaProperty);
			
			EditorGUILayout.PropertyField(fadeMaxAlphaProperty);
	
			EditorGUILayout.PropertyField(maxNewTargetsEachFrameProperty);
	
			EditorGUILayout.PropertyField(expandingTargetBoxesProperty);
			
			EditorGUILayout.EndVertical();
	
	
			// World space settings

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("World Space Settings", EditorStyles.boldLabel);

			EditorGUILayout.PropertyField(toggleWorldSpaceUIProperty);

			EditorGUILayout.PropertyField(useTargetWorldPositionsProperty);
		
			EditorGUILayout.PropertyField(worldSpaceVisorDistanceProperty);

			EditorGUILayout.PropertyField(worldSpaceScaleCoefficientProperty);

			EditorGUILayout.EndVertical();


			// Team colors
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
			
			for (int i = 0; i < script.colorByTeam.Count; ++i)
			{

				script.colorByTeam[i] = EditorGUILayout.ColorField(teamNames[i] + " Color", script.colorByTeam[i]);
				
			}

			EditorGUILayout.EndVertical();


			// Per-type widget settings

			for (int i = 0; i < script.widgetSettingsByType.Count; ++i)
			{

				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("TrackableType " + typeNames[i] + " Visualization Settings", EditorStyles.boldLabel);

				// prefab
				script.widgetSettingsByType[i].widgetPrefab = EditorGUILayout.ObjectField("Widget Prefab", script.widgetSettingsByType[i].widgetPrefab, typeof(GameObject),false) as GameObject;


				// Visualization settings 

				script.widgetSettingsByType[i].showOffScreenTargets = EditorGUILayout.Toggle("Show Off Screen Targets", script.widgetSettingsByType[i].showOffScreenTargets);
	
				script.widgetSettingsByType[i].fadeUnselectedByDistance = EditorGUILayout.Toggle("Fade By Distance", script.widgetSettingsByType[i].fadeUnselectedByDistance);

				// Visible parameters
				script.widgetSettingsByType[i].showLabelField = EditorGUILayout.Toggle("Show Label Field", script.widgetSettingsByType[i].showLabelField);
				script.widgetSettingsByType[i].showValueField = EditorGUILayout.Toggle("Show Value Field", script.widgetSettingsByType[i].showValueField);
				script.widgetSettingsByType[i].showBarField = EditorGUILayout.Toggle("Show Bar Field", script.widgetSettingsByType[i].showBarField);
				
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();

			}

			// Serialize the updated values
			EditorUtility.SetDirty(script);
			serializedObject.ApplyModifiedProperties();
			
	    }
	}
}

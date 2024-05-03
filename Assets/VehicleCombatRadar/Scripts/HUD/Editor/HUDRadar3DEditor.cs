using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.Vehicles;

// This script is to modify the inspector of the HUDRadar3D so that the user can set different parameters
// for displaying targets of different types

namespace VSX.Vehicles {

	[CustomEditor(typeof(HUDRadar3D))]
	public class HUDRadar3DEditor : Editor {
		
		HUDRadar3D script;

		void OnEnable()
		{
			script = (HUDRadar3D)target;
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


			// Settings

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

			script.equatorRadius = EditorGUILayout.FloatField("Equator Radius", script.equatorRadius);
	
			script.scaleExponent = EditorGUILayout.FloatField("Scale Exponent", script.scaleExponent);
	
			script.zoomSpeed = EditorGUILayout.FloatField("Zoom Speed", script.zoomSpeed);

			script.fadeMinAlpha = EditorGUILayout.FloatField("Fade By Distance Min Alpha", script.fadeMinAlpha);
	
		 	script.fadeMaxAlpha = EditorGUILayout.FloatField("Fade By Distance Max Alpha", script.fadeMaxAlpha);

			script.maxNewTargetsEachFrame = EditorGUILayout.IntField("Max New Targets Each Frame", script.maxNewTargetsEachFrame);

			EditorGUILayout.EndVertical();


			// Per-team colors
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
			
			for (int i = 0; i < script.colorByTeam.Count; ++i)
			{
				
				script.colorByTeam[i] = EditorGUILayout.ColorField(teamNames[i] + " Color", script.colorByTeam[i]);
				
			}

			EditorGUILayout.EndVertical();
			

			// Per-type settings

			for (int i = 0; i < script.widgetSettingsByType.Count; ++i)
			{
				EditorGUILayout.BeginVertical("box");
				EditorGUILayout.LabelField("TrackableType " + typeNames[i] + " Visualization Settings", EditorStyles.boldLabel);

				script.widgetSettingsByType[i].fadeUnselectedByDistance = EditorGUILayout.Toggle("Fade Unselected By Distance", script.widgetSettingsByType[i].fadeUnselectedByDistance);
		
				script.widgetSettingsByType[i].widgetPrefab = EditorGUILayout.ObjectField("Widget Prefab", script.widgetSettingsByType[i].widgetPrefab, typeof(GameObject), false) as GameObject;
            	
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
			}
			
			// serialize the updated values
			EditorUtility.SetDirty(script);
			serializedObject.ApplyModifiedProperties();
	    }
	}
}

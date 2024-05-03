using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using VSX.Vehicles;

// This script is to modify the inspector of the HUDManager so that the user can set style information according 
// to the teams in the RadarFunctions.Team enum.

namespace VSX.Vehicles {

	[CustomEditor(typeof(HUDHologramManager))]
	public class HUDHologramManagerEditor : Editor {
		
		public override void OnInspectorGUI()
		{

			// Setup
			serializedObject.Update();
			HUDHologramManager script = (HUDHologramManager)target;
			
			string[] teamNames = Enum.GetNames(typeof(RadarFunctions.Team));
	                
			RadarFunctions.ResizeList(script.colorByTeam, teamNames.Length);
			
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

			script.targetHologram = EditorGUILayout.ObjectField("Target Hologram Controller", script.targetHologram, typeof(HUDHologramController), true) as HUDHologramController;

			EditorGUILayout.EndVertical();


			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
			
			for (int i = 0; i < script.colorByTeam.Count; ++i)
			{
				
				script.colorByTeam[i] = EditorGUILayout.ColorField(teamNames[i] + " Color", script.colorByTeam[i]);
				
			}

			EditorGUILayout.EndVertical();

			EditorUtility.SetDirty(script);
			
			serializedObject.ApplyModifiedProperties();
	    }
	}
}

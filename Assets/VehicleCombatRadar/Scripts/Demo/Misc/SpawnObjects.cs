using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is a simple script to spawn targets for the demo and adjust the number of targets on the fly

namespace VSX.Vehicles {

	public class SpawnObjects : MonoBehaviour {
	
		public GameObject friendlyPrefab;
		public GameObject enemyPrefab;
	
		public int maxObjects;
		public float startingNumObjectsFraction;
		int numObjects = 0;
	
		RadarSceneManager radarSceneManager;
		List<ITrackable> trackables = new List<ITrackable>();

		ITrackable player;
	

		
		// Use this for initialization
		void Start()
		{
	
			radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			player = GameObject.FindGameObjectWithTag("Player").GetComponent<ITrackable>();
	
			for (int i = 0; i < maxObjects/2; ++i)
			{
				Instantiate(friendlyPrefab, Vector3.zero, Quaternion.identity);
				
				Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity);
				
			}
	
			SetNumObjects(startingNumObjectsFraction);
		}
	
		// Control the number of targets in the scene
		public void SetNumObjects(float fractionOfMax)
		{

			radarSceneManager.GetTrackablesInScene(trackables, false);

			numObjects = Mathf.Max((int)(fractionOfMax * trackables.Count), 0);
			int numObjectsActivated = 0;
			
			for (int i = 0; i < trackables.Count; ++i)
			{

				if (trackables[i] == player || trackables[i].Equals(null))
					continue;

				if (numObjectsActivated < numObjects)
				{
					trackables[i].GetGameObject().SetActive(true);
					numObjectsActivated += 1;
				}
				else
				{
					trackables[i].GetGameObject().SetActive(false);
				}
			}
		}

		public int GetNumObjects()
		{
			return numObjects;
		}
	}
}
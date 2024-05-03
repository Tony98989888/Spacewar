using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// This script controls a single object pool

namespace VSX.Generic {

	public class ObjectPool : MonoBehaviour {
	
		public  GameObject prefab;
	
		[SerializeField]
		private int startingAmount = 5;
	
		[SerializeField]
		private bool createOnAwake = false;
	
		List<GameObject> objectList = new List<GameObject>();

			
		void Awake()
		{

			// Create the starting amount of objects
			if (createOnAwake && prefab != null)
			{
				for (int i = 0; i < startingAmount; ++i)
				{
					CreateNewObject();
				}
			}
		}
	
		// Get an object from the pool
		public GameObject Get(Vector3 pos, Quaternion rot){
	
			// Search for an inactive object
			for (int i = 0; i < objectList.Count; ++i){
	
				// Prepare and deliver
				if (objectList[i].activeSelf == false){ // If the object is deactivated (not in use)
					objectList[i].transform.position = pos;
					objectList[i].transform.rotation = rot;
					objectList[i].SetActive (true);
					return objectList[i];
				}
			}
	
			// if an available one  was not found, create one
			GameObject newObject = CreateNewObject ();
			newObject.transform.position = pos;
			newObject.transform.rotation = rot;
			newObject.SetActive (true);
	
			return (newObject);
		}
	
		// Create a new object and add it to the list, and return the reference
		GameObject CreateNewObject(){

			GameObject newObject = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
			newObject.transform.SetParent(transform);
			newObject.SetActive (false);
			objectList.Add (newObject); 
			return newObject;	
		}
	}
}
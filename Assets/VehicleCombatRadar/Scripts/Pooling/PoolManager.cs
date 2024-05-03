using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.Vehicles;

// This script creates and manages the object pools in the scene

namespace VSX.Generic {

	public class PoolManager : MonoBehaviour {
	
		// List of all the pools attached to this manager
		List<ObjectPool> objectPools = new List<ObjectPool>();
	
		void Awake()
		{

			// Find all the pools already in the scene
			ObjectPool[] foundPools = GameObject.FindObjectsOfType<ObjectPool>();
			foreach (ObjectPool _pool in foundPools)
			{
				objectPools.Add(_pool);
			}

		}

		// Update is called once per frame
		public ObjectPool GetObjectPool(GameObject prefab)
		{
	
			// Check if there is already a pool for this object
			for (int i = 0; i < objectPools.Count; ++i){
				if (objectPools[i].prefab == prefab){
					return objectPools[i];
				}
			}
		
			// Create a pool for the prefab, and to keep the scene neat, parent it to the manager
			GameObject g = new GameObject (prefab.name + "Pool");
			g.transform.SetParent(transform);
	
			// Set up the pool
			ObjectPool o = g.AddComponent<ObjectPool>();
			o.prefab = prefab;
			
			// Add the object pool to the list
			objectPools.Add (o);
	
			return o;
		}
	}
}
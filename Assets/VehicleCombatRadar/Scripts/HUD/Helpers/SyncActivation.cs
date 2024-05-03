using UnityEngine;
using System.Collections;

// This class is for syncing the enabling/disabling of two objects

namespace VSX.Vehicles {

	public class SyncActivation : MonoBehaviour {
	
		[SerializeField]
		private GameObject otherGameObject;
	
		void OnEnable()
		{
			otherGameObject.SetActive(true);
		}
	
		void OnDisable()
		{
			otherGameObject.SetActive(false);
		}
	}
}

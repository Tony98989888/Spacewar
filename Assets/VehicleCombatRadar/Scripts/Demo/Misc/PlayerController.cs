using UnityEngine;
using System.Collections;

// This is a simple script to enable looking around in the demo

namespace VSX.Vehicles {

	public class PlayerController : MonoBehaviour {
	
		public float xSensitivity;
		public float ySensitivity;
		
		public bool disabled = false;
	
	
		// Look around

	 	public void LookRotation()
	    {
	
			if (disabled) return;
	
	        float yRot = Input.GetAxis("Mouse X") * xSensitivity;
	        float xRot = Input.GetAxis("Mouse Y") * ySensitivity;
	
			transform.Rotate(new Vector3 (-xRot, yRot, 0f));

	    }
	
		void Update()
		{
			LookRotation();
		}	
	
	}
}
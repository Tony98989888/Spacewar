using UnityEngine;
using System.Collections;
using VSX.Vehicles;

// This interface must be implemented by targets that are to be trackable

namespace VSX.Vehicles {

	// This interface is to ensure a common way to access properties of an object that is
	// trackable on radar. Such objects might be anything from waypoints to enemy ships.
	public interface ITrackable {
	
		string TrackableName{ get; }
		RadarFunctions.TrackableType TrackableType {get;}	
		bool IsSelectable{get;}

		Transform GetTransform();
		GameObject GetGameObject();
		Rigidbody GetRigidbody();

		bool HasBodyMesh {get;}
		Mesh BodyMesh { get; }
	
		RadarFunctions.Team Team{ get; }
		
		float GetCurrentHealth();
	
		float GetMaxHealth();

		bool HasHologramMesh {get;}
		Mesh HologramMesh{ get; }

		Texture2D HologramNormal{ get; }

	}
}

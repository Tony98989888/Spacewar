using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.Vehicles;

// This interface must be implemented by objects tracking ITrackables in the scene

namespace VSX.Vehicles {

	// This interface provides a path of information from a tracking object (such as a ship with a radar)
	// to other objects such as a HUD
	public interface ITracker {
	
		bool LinkToUI{ get; }

		float FrontTargetAngle{ get; }

		float Range{ get; }

		bool HasSelectedTarget{ get; }

		Vector3 SelectedTargetLeadPosition { get; }

		RadarFunctions.LockState CurrentLockState{ get; }

		void GetNewTarget(RadarFunctions.SelectionID selectionID, RadarFunctions.SelectionMode mode);
		
		ITrackable Trackable();

		List<ITrackable> GetTrackedTargets();
	
		ITrackable SelectedTarget{ get; }

		Transform GetTransform();
		
	}
}

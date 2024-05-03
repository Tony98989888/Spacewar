using UnityEngine;
using System.Collections;

// This interface must be implemented by a weapons script in order to give an ITracker component the correct
// information for lead target calculation and missile locking

namespace VSX.Vehicles {

	public interface IWeaponInfo {
	
		float GetProjectileSpeed();
	
		float GetMissileRange();
		float GetMissileLockingTime();
		float GetMissileMaxLockingAngle();
	
	}
}

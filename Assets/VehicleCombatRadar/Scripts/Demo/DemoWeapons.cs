using UnityEngine;
using System.Collections;
using VSX.Vehicles;

// This is an example of how to implement the IWeaponInfo interface which is used by the ITracker component
// to calculate lead target position and locking information.

namespace VSX.Vehicles {

	public class DemoWeapons : MonoBehaviour, IWeaponInfo {
	
		[SerializeField]
		private float projectileSpeed = 300;
	
		[SerializeField]
		private float missileRange = 500;
		
		[SerializeField]
		private float missileLockingTime = 2;
	
		[SerializeField]
		private float missileMaxLockingAngle = 15;
	
		
		// Get the shot speed for lead target calculation
		public float GetProjectileSpeed()
		{
			return projectileSpeed;
		}
	
		// Get the missile range for locking 
		public float GetMissileRange()
		{
			return missileRange;
		}
	
		// Get missile locking time
		public float GetMissileLockingTime()
		{
			return missileLockingTime;
		}
	
		// Get missile max locking angle
		public float GetMissileMaxLockingAngle()
		{
			return missileMaxLockingAngle;
		}
	}
}
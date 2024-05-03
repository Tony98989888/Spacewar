using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// This script is designed to show how the ITracker interface should be implemented, and how to implement
// target selection, lead target tracking, and locking.


namespace VSX.Vehicles {

	[RequireComponent(typeof(DemoTrackable))]
	public class DemoRadar : MonoBehaviour, ITracker  {
	
		[Header("Parameters")]

		[SerializeField]
		private bool linkToUI = true;
		public bool LinkToUI
		{
			get
			{
				return linkToUI;
			}
		}

		[SerializeField]
		private float range = 1000;
		public float Range
		{
			get
			{
				return range;
			}
		}

		[Header("Targeting")]

		[SerializeField]
		public float frontTargetAngle = 10f;
		public float FrontTargetAngle
		{
			get
			{
				return frontTargetAngle;
			}	
		}

		ITrackable selectedTarget;
		public ITrackable SelectedTarget
		{
			get
			{
				return (selectedTarget);
			}
		}

		bool hasSelectedTarget;
		public bool HasSelectedTarget
		{
			get
			{
				return (hasSelectedTarget);
			}
		}

		private Vector3 selectedTargetLeadPosition;
		public Vector3 SelectedTargetLeadPosition
		{
			get
			{
				return selectedTargetLeadPosition;	
			}
		}
		
		List<ITrackable> trackedTargets = new List<ITrackable>();

		RadarSceneManager radarSceneManager;
		bool hasRadarSceneManager;
	
		public RadarFunctions.SelectionMode defaultSelectionMode = RadarFunctions.SelectionMode.Next;

		ITrackable previousTarget = null;

		ITrackable thisTrackable;
	
		IWeaponInfo weaponInfo;
		bool hasWeaponInfo = false;

		private RadarFunctions.LockState currentLockState;
		public RadarFunctions.LockState CurrentLockState
		{
			get
			{
				return currentLockState;
			}
		}
	
		[Header("Audio")]

		public AudioClip switchTargetAudioClip;
		bool hasSwitchTargetAudio = false;

		public AudioClip lockingAudioClip;
		bool hasLockingAudio = false;

		public AudioClip lockedAudioClip;
		bool hasLockedAudio = false;	

		[SerializeField]
		private AudioSource radarAudioSource;
		bool hasAudio = false;



		void Awake()
		{
			// Register this radar in the manager
			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager != null)
			{
				radarSceneManager.Register(this);
				hasRadarSceneManager = true;
			}
			else
			{
				Debug.LogError("Please add a RadarSceneManager component to the scene, cannot register this ITracker");
			}
		}
		
		void Start()
		{

			// Get the trackable associated with this tracker
			thisTrackable = transform.GetComponentInChildren<ITrackable>();

			radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			StartCoroutine(LockSequence());

			// Get the weapon info interface
			weaponInfo = transform.root.GetComponentInChildren<IWeaponInfo>();
			if (weaponInfo == null)
			{
				Debug.LogWarning("No IWeaponInfo component assigned to the object hierarchy of the ITracker component, missile locking and lead target calculations are disabled");
				hasWeaponInfo = false;
			}
			else
			{
				hasWeaponInfo = true;
			}

			// Audio checks
			hasSwitchTargetAudio = (switchTargetAudioClip != null);
			hasLockingAudio = (lockingAudioClip != null);
			hasLockedAudio = (lockedAudioClip != null);

			hasAudio = (radarAudioSource != null);
			
		}

		void OnDisable(){
			trackedTargets.Clear();
		}

		// Returns the ITrackable component of this ship/vehicle
		public ITrackable Trackable()
		{
			return thisTrackable;
		}

		// Return the list of tracked targets
		public List<ITrackable> GetTrackedTargets(){
			return (trackedTargets);
		}
	
		// Return the transform of this ITracker
		public Transform GetTransform(){
			return transform;
		}
	
		// Determine whether the radar should begin locking sequence on the selected target
		bool IsInLockZone(){
	
			if (selectedTarget == null) return false;
			float angleToTarget = Vector3.Angle(Vector3.forward, transform.InverseTransformPoint(selectedTarget.GetTransform().position));
			float distToTarget = Vector3.Distance(selectedTarget.GetTransform().position, transform.position);
	
			return (angleToTarget < weaponInfo.GetMissileMaxLockingAngle() && 
					distToTarget < weaponInfo.GetMissileRange());
	
		}
	
		// Coroutine for updating weapon lock on selected target
		IEnumerator LockSequence()
		{
			
			float lockStartTime = Time.time;
	
			while (true)
			{
				if (!hasWeaponInfo)
				{ 
					yield return null;
					continue;
				}

				switch (currentLockState)
				{
	
					case RadarFunctions.LockState.NoLock:
						if (IsInLockZone())
						{
							lockStartTime = Time.time;
							currentLockState = RadarFunctions.LockState.Locking;

							// Play audio
							if (hasAudio)
							{
								radarAudioSource.Stop();
								if (hasLockingAudio)
								{
									radarAudioSource.clip = lockingAudioClip;
									radarAudioSource.loop = true;
									radarAudioSource.Play();
								}
							}
						}
						break;
	
					case RadarFunctions.LockState.Locking:
						if (!IsInLockZone())
						{
							if (hasAudio) radarAudioSource.Stop();
							currentLockState = RadarFunctions.LockState.NoLock;
						}
						else
						{
							if (Time.time - lockStartTime > weaponInfo.GetMissileLockingTime())
							{
								currentLockState = RadarFunctions.LockState.Locked;

								// Play audio
								if (hasAudio)
								{
									radarAudioSource.Stop();
									if (hasLockedAudio)
									{ 
										radarAudioSource.PlayOneShot(lockedAudioClip);
									}
								}
							}
						}
						break;
	
					case RadarFunctions.LockState.Locked:
						if (!IsInLockZone()){
							currentLockState = RadarFunctions.LockState.NoLock;
						}
						break;
				
				}
				yield return null;
			}
		}

		// Update the lead target position for visualisation
		public void UpdateLeadTargetPosition()
		{
			if (!hasSelectedTarget || selectedTarget.Equals(null)) return;

			if (hasWeaponInfo && weaponInfo.GetProjectileSpeed() > 0.0001f)
			{
				selectedTargetLeadPosition = RadarFunctions.GetLeadPosition(transform.position, weaponInfo.GetProjectileSpeed(), selectedTarget);
			}
			else
			{
				selectedTargetLeadPosition = selectedTarget.GetTransform().position;
			}
		}


		// If target is not in the targets list, is not trackable (e.g. out of range), or has been destroyed, get a new one
		void UpdateSelectedTargetStatus()
		{
			if (selectedTarget == null || selectedTarget.Equals(null) || trackedTargets.IndexOf(selectedTarget) == -1)
			{	
				selectedTarget = null;
				hasSelectedTarget = false;
			}
			else
			{
				hasSelectedTarget = true;
			}
		}

		// Get a new target
		public void GetNewTarget(RadarFunctions.SelectionID selectionID, RadarFunctions.SelectionMode mode)
		{
	
			if (thisTrackable == null)
				return;

			UpdateSelectedTargetStatus();

			selectedTarget = RadarFunctions.GetNewTarget(this, trackedTargets, selectionID, mode);
			
			hasSelectedTarget = (selectedTarget != null); 
			
			if (selectedTarget != previousTarget)
			{

				// Update lead target pos
				UpdateLeadTargetPosition();

				// Play audio
				if (hasAudio && hasSwitchTargetAudio)
					radarAudioSource.PlayOneShot(switchTargetAudioClip);

				// Update previous
				previousTarget = selectedTarget;

				// Reset the lock
				currentLockState = RadarFunctions.LockState.NoLock;
				if (hasAudio)
					radarAudioSource.Stop();

			}
		}
	
		// Update
		void Update()
		{
			
			if (!hasRadarSceneManager)
				return;
			
			// Update targets
			radarSceneManager.GetTrackablesInRange(trackedTargets, range, this);

			//Update the status of the selected target
			UpdateSelectedTargetStatus();
			
			if (!hasSelectedTarget)
			{
				GetNewTarget(RadarFunctions.SelectionID.Any, defaultSelectionMode);
			}
			else
			{
				UpdateLeadTargetPosition();
			}
		}
	}
}

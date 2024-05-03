using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VSX.Generic;

// This script registers and tracks all the ITrackable objects in the scene and provides ITracker objects with a list of the
// ITrackable objects in range

namespace VSX.Vehicles {

	[RequireComponent(typeof(RadarDistanceLookup))]
	[RequireComponent(typeof(PoolManager))]
	public class RadarSceneManager : MonoBehaviour {
	
		// A list of all the trackables in the scene
		List<ITrackable> trackables = new List<ITrackable>();
		
		
		private ITracker uiTracker;
		public ITracker UITracker
		{
			get
			{
				return uiTracker;
			}
		}
	
		List<ITracker> trackers = new List<ITracker>();
		
		public void Register(ITracker _tracker)
		{
			trackers.Add(_tracker);
			if ((uiTracker == null) && _tracker.LinkToUI)
			{
				uiTracker = _tracker;
			}
		}

		// Register a new trackable object
		public void Register(ITrackable newTrackable)
		{
			trackables.Add(newTrackable);
		}
		
		// Unregister a trackable object
		public void Unregister(ITrackable removedTrackable)
		{
			int index = trackables.IndexOf(removedTrackable);
			
			if (index != -1) trackables.RemoveAt(index);
		}

		// Cleanly destroy a trackable object in the scene
		public void DestroyTrackable(ITrackable trackableToDestroy)
		{
	
			// Unregister
			Unregister(trackableToDestroy);
			
			// Start a coroutine to destroy it
			StartCoroutine(DestroyTrackableCoroutine(trackableToDestroy));			

		}

        // Coroutine for destroying trackable object after 1 frame
		IEnumerator DestroyTrackableCoroutine(ITrackable trackableToDestroy)
		{

			trackableToDestroy.GetGameObject().SetActive(false);

			// Wait one frame before destroying
			yield return null;
			
			Destroy(trackableToDestroy.GetGameObject());

		}
	
		// Get the trackables within a certain range of a location
		public void GetTrackablesInRange(List<ITrackable> targetsList, float range, ITracker tracker)
		{
			
			// Reference to the last used index in the list to update, for use when trimming excess off the end
			int usedIndex = -1;

			for (int i = 0; i < trackables.Count; ++i)
			{
				if (!trackables[i].Equals(null) && trackables[i].GetGameObject().activeSelf && Vector3.Distance(tracker.GetTransform().position, 
					    trackables[i].GetTransform().position) < tracker.Range)
				{
					
					usedIndex += 1;

					if (usedIndex >= targetsList.Count)
					{

						targetsList.Add(trackables[i]);
						
					}
					else
					{

						targetsList[usedIndex] = trackables[i];
						
					}
				}
			}
			
			// Remove excess references
			if (targetsList.Count > usedIndex + 1)
			{
				int removeAmount = targetsList.Count - (usedIndex + 1);
				targetsList.RemoveRange(usedIndex + 1, removeAmount);
			}
		}

		// Get the trackables within a certain range of a location
		public void GetTrackablesInScene(List<ITrackable> targetsList, bool getOnlyActive)
		{
			
			// Reference to the last used index in the list to update, for use when trimming excess off the end
			int usedIndex = -1;

			for (int i = 0; i < trackables.Count; ++i)
			{

				if (trackables[i].Equals(null) || getOnlyActive && !trackables[i].GetGameObject().activeSelf)
				{
					continue;
				}

				usedIndex += 1;

				if (usedIndex >= targetsList.Count)
				{

					targetsList.Add(trackables[i]);
					
				}
				else
				{

					targetsList[usedIndex] = trackables[i];
					
				}
			}
			
			// Remove excess references
			if (targetsList.Count > usedIndex + 1)
			{
				int removeAmount = targetsList.Count - (usedIndex + 1);
				targetsList.RemoveRange(usedIndex + 1, removeAmount);
			}
		}

	}
}

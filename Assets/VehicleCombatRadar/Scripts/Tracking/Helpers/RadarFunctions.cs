using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is a static class to store information and functionality that is used amongst a variety of radar scripts 

namespace VSX.Vehicles {

	public static class RadarFunctions {
	
		// Lock state of the radar
		public enum LockState{
			NoLock,
			Locking,
			Locked
		}
	
		// The search mode for finding a new target
		public enum SelectionMode{
			Next,
			Nearest,
			Front
		}

		// The characteristics of the desired target to be selected
		public enum SelectionID{
			Hostile,
			NonHostile,
			Any
		}

		// The team that an ITrackable belongs to
		public enum Team
		{
			Friendly,
			Enemy
		}

		// Different trackable types
		public enum TrackableType{
			Ship,
			Waypoint
		}

		// Resize a generic list
		public static void ResizeList<T>(List<T> list, int newSize)
	    {
	        if (list.Count == newSize)
	            return;
	            
	        if (list.Count < newSize)
	        {
	            for (int i = 0; i < newSize - list.Count; ++i)
	            {
					list.Add(default(T));
	            }
	        } else {
	            for (int i = 0; i < list.Count - newSize; ++i){
	                //Remove the last one in the list
	                list.RemoveAt(list.Count-1);
	                --i;
	            }
	        }
	    }

		// Get a new target for an ITracker component
		public static ITrackable GetNewTarget(ITracker tracker, List<ITrackable> targetsList, RadarFunctions.SelectionID selectionID, RadarFunctions.SelectionMode mode)
		{

			// Find the team that the new target will be on
			RadarFunctions.Team targetTeam;
			switch (selectionID)
			{
				case RadarFunctions.SelectionID.Hostile:
					if (tracker.Trackable().Team == RadarFunctions.Team.Friendly)
					{
						targetTeam = RadarFunctions.Team.Enemy;
					}
					else
					{
						targetTeam = RadarFunctions.Team.Friendly;
					}
					break;

				default:
					targetTeam = tracker.Trackable().Team;
					break;
			}

			// The returned result
			ITrackable newTarget = null;

			switch (mode)
			{
	
				case RadarFunctions.SelectionMode.Next:
	
					// If the ship's had no previous target, just use the first active hostile target
					if (!tracker.HasSelectedTarget)
					{ 
						for (int i = 0; i < targetsList.Count; ++i)
						{

							// Check attributes
							if (targetsList[i] == tracker.Trackable()) continue;	
							if (!targetsList[i].IsSelectable) continue;			
							if (selectionID != RadarFunctions.SelectionID.Any && targetsList[i].Team != targetTeam) continue;
							if (targetsList[i] == tracker.SelectedTarget) continue;
							if (targetsList[i].Equals(null)) continue;
							newTarget = targetsList[i];
							break;
						}
					}
					else
					{
		
						int currentTargetIndex = Mathf.Max(0, targetsList.IndexOf(tracker.SelectedTarget));
						bool found = false;

						// Check for an active hostile from the current index to the end
						for (int i = currentTargetIndex; i < targetsList.Count; ++i)
						{

							// Check attributes
							if (targetsList[i] == tracker.Trackable()) continue;	
							if (!targetsList[i].IsSelectable) continue;			
							if (selectionID != RadarFunctions.SelectionID.Any && targetsList[i].Team != targetTeam) continue;
							if (targetsList[i] == tracker.SelectedTarget) continue;
							if (targetsList[i].Equals(null)) continue;

							newTarget = targetsList[i];
							found = true;
							break;
						}
		
						// Check for an active hostile from the start to the current index (inclusive)
						if (!found)
						{
							for (int i = 0; i <= currentTargetIndex; ++i)
							{

								// Check attributes
								if (targetsList[i] == tracker.Trackable()) continue;	
								if (!targetsList[i].IsSelectable) continue;			
								if (selectionID != RadarFunctions.SelectionID.Any && targetsList[i].Team != targetTeam) continue;
								if (targetsList[i].Equals(null)) continue;
								
								newTarget = targetsList[i];
								break;

							}	
						}	
					}
					break;
				
				case RadarFunctions.SelectionMode.Nearest:
	
					int closestTargetIndex = -1;
					float minDistance = tracker.Range + 1; // More than possible

					for (int i = 0; i < targetsList.Count; ++i)
					{

						// Check attributes
						if (targetsList[i] == tracker.Trackable()) continue;	
						if (!targetsList[i].IsSelectable) continue;			
						if (selectionID != RadarFunctions.SelectionID.Any && targetsList[i].Team != targetTeam) continue;
						if (targetsList[i].Equals(null)) continue;

						// Check if it is the nearest
						float distance = Vector3.Distance(tracker.GetTransform().position, targetsList[i].GetTransform().position);
						if (distance < minDistance)
						{
							minDistance = distance;
							closestTargetIndex = i;
						}
					}

					// update the result
					if (closestTargetIndex == -1)
					{
						newTarget = null;
					}
					else
					{
						newTarget = targetsList[closestTargetIndex];
					}
					break;
	
				case RadarFunctions.SelectionMode.Front:

					ITrackable targetUnderNose = tracker.SelectedTarget;
			
					float minAngle = 181f; // More than maximum possible

					for (int i = 0; i < targetsList.Count; ++i){

						// Check attributes
						if (targetsList[i] == tracker.Trackable()) continue;	
						if (!targetsList[i].IsSelectable) continue;			
						if (selectionID != RadarFunctions.SelectionID.Any && targetsList[i].Team == targetTeam) continue;
						if (targetsList[i].Equals(null)) continue;
					
						// Check if the angle to the target is smaller
	                    float angle = Vector3.Angle (tracker.GetTransform().forward, targetsList[i].GetTransform().position - tracker.GetTransform().position);
						if (angle <= tracker.FrontTargetAngle && angle < minAngle){
							minAngle = angle;
							targetUnderNose = targetsList[i];
						}

					}
					newTarget = targetUnderNose;
					break;
	
			}
			return newTarget;
		}

	
		// Calculate the lead position at which to aim to intercept a moving target
		public static Vector3 GetLeadPosition(Vector3 shooterPosition, float interceptSpeed, ITrackable target){
	
			// Get target direction
			Vector3 targetDirection = target.GetTransform().position - shooterPosition;
			
			// Get the target velocity magnitude
			float vE = target.GetRigidbody().velocity.magnitude;
			
			// Note that the dot product of a and b (a.b) is also equal to |a||b|cos(theta) where theta = angle between them.
			// This saves using the components of the vectors themselves, only the magnitudes.
			
			// Get the length of the playerRelPos vector
			float playerRelPosMag = targetDirection.magnitude;
			
			// Get angle between player-target axis and the target's velocity vector
			float angPET = Vector3.Angle (targetDirection, target.GetRigidbody().velocity)*Mathf.Deg2Rad; // Vector3.Angle returns in degrees
			
			// Get the cosine of this angle
			float cosPET = Mathf.Cos (angPET);			
			
			// Get the coefficients of the quadratic equation
			float a = vE*vE - interceptSpeed*interceptSpeed;
			float b = 2 * playerRelPosMag * vE * cosPET;
			float c = playerRelPosMag * playerRelPosMag;
			
			// Check for solutions. If the discriminant (b*b)-(4*a*c) is:
			// >0: two real solutions - get the maximum one (the other will be a negative time value and can be discarded)
			// =0: one real solution (both solutions the same so either one will do)
			// <0; two imaginary solutions - never will hit the target
			float discriminant = (b*b)-(4*a*c);
			
			// Get the intercept time by solving the quadratic equation. Note that if a = 0 then we will be 
			// trying to divide by zero. But in that case no quadratics are necessary, the equation will be first-order
			// and can simply be rearranged to get the intercept time.
			
			float interceptTime;	
			if (a != 0)
				interceptTime = Mathf.Max (((-1*b) - Mathf.Sqrt (discriminant))/(2*a), ((-1*b) + Mathf.Sqrt (discriminant))/(2*a));
			else {
				interceptTime = -c/(2*b);
			}
			
			return(target.GetTransform().position + (target.GetRigidbody().velocity * interceptTime));	
		}
	}
}

using UnityEngine;
using System.Collections;

// This is a simple script to give the ships in the demo random movement around the origin

namespace VSX.Vehicles {

	public class RandomMovement : MonoBehaviour {
	
		[SerializeField]
		private Vector2 minMaxVelocity;
		float currentVelocity;

		Rigidbody rBody;
	
		[SerializeField]
		private float maxTravelTime;
		float startTime;

		[SerializeField]
		private float boundaryRadius;			// The radius around origin that new destinations are generated within
		Vector3 destination;

		[SerializeField]
		private float maxArrivalProximity;		// maximum distance to destination that is considered an arrival
		
	
	
		public void Awake()
		{
			rBody = GetComponent<Rigidbody>();
			Vector3 spawnPos = Random.insideUnitSphere.normalized * Random.Range(0f, boundaryRadius);
			transform.position = spawnPos;
			destination = spawnPos;
		}
	
		// Grab a new point in space to head toward
		void GetNewDestination(){

			destination = Random.insideUnitSphere.normalized * Random.Range(0f, boundaryRadius);
			currentVelocity = Random.Range(minMaxVelocity.x, minMaxVelocity.y);
			startTime = Time.time;

		}

		public void Update()
		{

			// Check if arrived
			float distToDestination = Vector3.Distance(transform.position, destination);
			bool hasArrived = (distToDestination < maxArrivalProximity) || (Time.time - startTime > maxTravelTime);

			if (hasArrived)
			{
				GetNewDestination();
			}
	
			// Rotate toward destination
			Quaternion targetRotation = Quaternion.LookRotation(destination - transform.position, Vector3.up);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
		}

		void FixedUpdate()
		{
			rBody.velocity = transform.forward * currentVelocity;
		}
	}
}
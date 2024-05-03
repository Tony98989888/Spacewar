using UnityEngine;
using System.Collections;
using VSX.Vehicles;

// This script is an example of how to implement the ITrackable interface, which makes the object that it 
// is assigned to trackable.

namespace VSX.Vehicles {

	[RequireComponent(typeof(Rigidbody))]
	public class DemoTrackable : MonoBehaviour, ITrackable {
	
		[SerializeField]
		private string trackableName = "IAmTrackable";
		public string TrackableName
		{
			get
			{
				return trackableName;
			}
		}
		
		[SerializeField]
		private bool isSelectable = true;
		public bool IsSelectable
		{
			get
			{
				return isSelectable;
			}
		}

		[SerializeField]
		private RadarFunctions.Team team;
		public RadarFunctions.Team Team
		{
			get
			{
				return team;
			}
		}

		[SerializeField]
		private RadarFunctions.TrackableType trackableType;
		public RadarFunctions.TrackableType TrackableType
		{
			get
			{
				return trackableType;
			}
		}
	
		[SerializeField]
		private bool hasBodyMesh = false;
		public bool HasBodyMesh
		{
			get
			{
				return hasBodyMesh;
			}
		}

		[SerializeField]
		private Mesh bodyMesh;
		public Mesh BodyMesh
		{
			get
			{
				return bodyMesh;
			}
		}

		[SerializeField]
		private Mesh hologramMesh;
		public Mesh HologramMesh
		{
			get
			{
				return hologramMesh;
			}
		}

		[SerializeField]
		private bool hasHologramMesh = false;
		public bool HasHologramMesh
		{
			get
			{
				return hasHologramMesh;
			}
		}

		[SerializeField]
		private Texture2D hologramNormal;
		public Texture2D HologramNormal
		{
			get
			{
				return hologramNormal;
			}
		}

		Rigidbody rBody;
	


		void Awake()
		{

			RadarSceneManager radarSceneManager = GameObject.FindObjectOfType<RadarSceneManager>();
			if (radarSceneManager != null)
			{
				radarSceneManager.Register(this);
			}
			else
			{
				Debug.LogError("Please add a RadarSceneManager component to the scene, cannot register this ITrackable");
			}

			rBody = GetComponent<Rigidbody>();

			// Try to find a mesh filter on this object and its children
			MeshFilter meshFilter = transform.GetComponentInChildren<MeshFilter>();

			if (meshFilter != null)
			{

				// Try to find a mesh for the body mesh
				if (bodyMesh == null)
				{
					bodyMesh = meshFilter.sharedMesh;
					hasBodyMesh = true;
				}
				else
				{
					hasBodyMesh = true;
				}
	
				// Try to find a mesh for the hologram mesh
				if (hologramMesh == null)
				{
					hologramMesh = meshFilter.sharedMesh;
					hasHologramMesh = true;				
				}
				else
				{
					hasHologramMesh = true;		
				}

				// Try to find a normal texture for the hologram mesh
				MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
				if (meshRenderer != null){
					if (hologramNormal == null && meshRenderer.material != null)
					{
						Texture tex = meshRenderer.material.GetTexture("_BumpMap");
						if (tex != null) hologramNormal = (Texture2D) tex;
					}
				}
			}
		}
	
		// Get the transform
		public Transform GetTransform()
		{

			return transform;

		}
	
		// Get the cached gameobject
		public GameObject GetGameObject()
		{
			return gameObject;
		}
		
		// Get the cached rigidbody
		public Rigidbody GetRigidbody()
		{
			return rBody;
		}
	
		// Get the current health
		public float GetCurrentHealth(){
			return 1f;
		}
	
		// Get the max health
		public float GetMaxHealth()
		{
			return 1f;
		}
	
		// Get the name
		public string GetName()
		{
			return trackableName;
		}
	}
}
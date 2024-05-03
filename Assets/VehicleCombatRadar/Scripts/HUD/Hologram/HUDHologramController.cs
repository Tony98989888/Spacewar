using UnityEngine;
using System.Collections;

// This class is for creating a hologram of an trackable object and updating it with the trackable object's
// relative orientation

namespace VSX.Vehicles{

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]

	public class HUDHologramController : MonoBehaviour {
	
		// Cached components
		MeshFilter hologramMeshFilter;
		MeshRenderer hologramMeshRenderer;
		Material hologramMat;
	
		[SerializeField]
		private float hologramSize = 0.3f;

		// This is for making the outline brighter (less saturated) than the middle
		[SerializeField]
		private float outlineSaturationCoefficient = 0.5f;

		[SerializeField]
		public SpriteRenderer platform;

		[SerializeField]
		private float platformSaturationCoefficient = 0.8f;

	

		void Awake()
		{

			// Cache components
			hologramMeshFilter = GetComponent<MeshFilter>();
			hologramMeshRenderer = GetComponent<MeshRenderer>();
			hologramMat = hologramMeshRenderer.material;
			
			Disable();
		}
	
		// Set a new mesh for the hologram
		public void Set(Mesh newMesh, Texture2D _normal)
		{

			hologramMeshFilter.sharedMesh = newMesh;
			if (_normal != null) hologramMat.SetTexture("_NormalMap", _normal);

			// Adjust the scale according to the size parameter set in inspector
			Vector3 extents = hologramMeshFilter.sharedMesh.bounds.extents;
	        float greatestDimension = Mathf.Max (new float []{extents.x, extents.y, extents.z});
	        float scale = hologramSize/greatestDimension;
			transform.localScale = new Vector3(scale, scale, scale);
			
		}
	
		// Enable the hologram
		public void Enable()
		{
			hologramMeshRenderer.enabled = true;
	    }
	
		// Disable the hologram
	    public void Disable()
	    {
	        hologramMeshRenderer.enabled = false;
	    }
	
		// Set the hologram color
		public void SetColor(Color col)
		{
			hologramMat.SetColor("_RimColor", col);

			float h, s, v;
			Color.RGBToHSV(col, out h, out s, out v);
			float _s = outlineSaturationCoefficient * s;
			Color outlineColor = Color.HSVToRGB(h, _s, v);
			
			hologramMat.SetColor("_OutlineColor", outlineColor);

			_s = platformSaturationCoefficient * s;
			Color platformColor = Color.HSVToRGB(h, _s, v);

			platform.color = platformColor;

		}
	
		// Update the hologram orientation. The orientation shows whether the ship in the hologram is facing 
		//the player, regardless of it's relative position
		public void UpdateHologram(Transform targetTransform, Transform trackerTransform)
		{
			
			// Calculate orientation
			Vector3 direction = (targetTransform.position - trackerTransform.position).normalized;
	        Quaternion temp = Quaternion.LookRotation(direction, trackerTransform.up);
	        Quaternion rot = Quaternion.Inverse(Quaternion.Inverse(targetTransform.rotation) * temp);
	        transform.localRotation = rot;   

		}	
	}
}
using UnityEngine;
using System.Collections;

// This class is for caching often-used components of a Sprite object

namespace VSX.Vehicles {

	public class HUDSpriteObject {
	
		public GameObject cachedGO;
		public Transform cachedTransform;
		public SpriteRenderer cachedSpriteRenderer;
	
		public HUDSpriteObject(GameObject _gameObject, Transform _transform, SpriteRenderer _spriteRenderer)
		{
			cachedGO = _gameObject;
			cachedTransform = _transform;
			cachedSpriteRenderer = _spriteRenderer;
		}
	}
}

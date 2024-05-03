using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// This class is for caching often-used components of an Image UI object

namespace VSX.Vehicles {

	public class HUDImageObject {
	
		public RectTransform cachedRT;
		public GameObject cachedGO;
		public Image cachedImage;
		
		public HUDImageObject(Image _image)
		{
			cachedImage = _image;
			cachedGO = _image.gameObject;
			cachedRT = _image.GetComponent<RectTransform>();
		}
	}
}
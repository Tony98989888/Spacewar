using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// This class is for caching often-used components of a Text UI object

namespace VSX.Vehicles {

	public class HUDTextObject {
	
		public RectTransform cachedRT;
		public GameObject cachedGO;
		public Text cachedText;
		
		public HUDTextObject(Text _text)
		{
			cachedText = _text;
			cachedGO = _text.gameObject;
			cachedRT = _text.GetComponent<RectTransform>();
		}
	}
}
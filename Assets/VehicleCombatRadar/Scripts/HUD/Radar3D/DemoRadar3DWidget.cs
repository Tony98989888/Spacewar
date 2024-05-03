using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using VSX.Vehicles;

// This script controls a widget for a 3D radar, including setting the position and style for all of the widget
// components

namespace VSX.Vehicles {

	public class DemoRadar3DWidget : MonoBehaviour, IRadar3DWidget {

		[SerializeField]
		private SpriteRenderer iconSprite;
		private HUDSpriteObject iconSpriteObject;
		
		[SerializeField]
		private LineRenderer legLineRenderer;
		private Material legMaterial;
		private HUDSpriteObject legSpriteObject; 
	    
		[SerializeField]
		private SpriteRenderer footSprite;
		private HUDSpriteObject footSpriteObject;
	
		[SerializeField]
		private SpriteRenderer selectedSprite;
		private HUDSpriteObject selectedSpriteObject;
	    
	
	    void Awake()
	    {
			
			// Create easy-access class instances to modify the parameters quicklu

	        iconSpriteObject = new HUDSpriteObject(iconSprite.gameObject, iconSprite.transform, iconSprite);
	            
			legMaterial = legLineRenderer.material; 
	
			footSpriteObject = new HUDSpriteObject(footSprite.gameObject, footSprite.transform, footSprite);
	
			selectedSpriteObject = new HUDSpriteObject(selectedSprite.gameObject, selectedSprite.transform, selectedSprite);
			
	    }
	
		// Anything that needs to be done before enabling
	    public void Enable()
	    {
	        gameObject.SetActive(true);
	    }
	
		// Anything that needs to be done before disabling
	    public void Disable()
	    {
	        gameObject.SetActive(false);
	    }
	 
		// Set the widget according to the parameters passed by the HUDRadar3D component
		public void Set(Radar3D_WidgetParameters _parameters){
			
			// Position
			iconSpriteObject.cachedTransform.localPosition = _parameters.widgetLocalPosition;
	
			// Selected
			if (_parameters.isSelected)
			{
				selectedSpriteObject.cachedGO.SetActive(true);
			}
			else
			{
				selectedSpriteObject.cachedGO.SetActive(false);
			}
	
			// Leg			
			legLineRenderer.SetPosition(0, _parameters.widgetLocalPosition);
			legLineRenderer.SetPosition(1, _parameters.widgetLocalPosition + new Vector3(0f, -_parameters.widgetLocalPosition.y, 0f));
	
	        // Foot
	        footSpriteObject.cachedTransform.localPosition = _parameters.widgetLocalPosition + new Vector3(0f, -_parameters.widgetLocalPosition.y, 0f);

			// Color
			Color col = _parameters.widgetColor;
			col.a = _parameters.alpha;

			iconSpriteObject.cachedSpriteRenderer.color = col;
	        footSpriteObject.cachedSpriteRenderer.color = col;
			selectedSpriteObject.cachedSpriteRenderer.color = col;
			legMaterial.SetColor("_Color", col);

		}
	}
}

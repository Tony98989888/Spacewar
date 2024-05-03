using UnityEngine;
using System.Collections;

// This interface allows widgets of many different kinds to be controlled by the HUDRadar3D component, through 
// the Radar3D_WidgetParameters class

namespace VSX.Vehicles{

	public interface IRadar3DWidget {
	
		void Enable();
	
		void Disable();
	
		void Set(Radar3D_WidgetParameters _parameters);
	
	}
}

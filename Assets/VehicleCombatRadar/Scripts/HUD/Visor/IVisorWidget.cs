using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This interface allows widgets of many different kinds to be controlled by the HUDVisor component, through 
// the Visor_WidgetParameters class

namespace VSX.Vehicles {

	public interface IVisorWidget {
	
		void Enable();

		void Disable();

		void Set(Visor_WidgetParameters _parameters);

	}

}

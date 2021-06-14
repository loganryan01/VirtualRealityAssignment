using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CustomDirectInteractor : XRDirectInteractor
{


    public void ResetValidTargets()
	{
		validTargets.Clear();
	}
}

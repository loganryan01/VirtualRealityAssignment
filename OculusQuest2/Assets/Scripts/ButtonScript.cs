using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonScript : MonoBehaviour
{
    
    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;

    
    public Text controlsText;

    public void ChangeControls()
    {
        //teleportationProvider.enabled = !teleportationProvider.enabled;
        //continuousMoveProvider.enabled = !continuousMoveProvider.enabled;

        //if (teleportationProvider.enabled)
        //{
        //    controlsText.text = "Teleportation";
            
        //    //leftRay.interactionLayerMask = LayerMask.NameToLayer("Everything");
        //    //rightRay.interactionLayerMask = LayerMask.NameToLayer("Everything");
        //}
        //else
        //{
        //    controlsText.text = "Joystick";
        //    //leftRay.interactionLayerMask = LayerMask.NameToLayer("UI");
        //    //rightRay.interactionLayerMask = LayerMask.NameToLayer("UI");
        //}

        //PlayerPrefs.SetString("Controls", controlsText.text);
        //PlayerPrefs.Save();
    }
}

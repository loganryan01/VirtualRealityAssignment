using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RollingBallScript : MonoBehaviour
{

    XRGrabInteractable grabScript;

    void Start()
    {
        grabScript = GetComponent<XRGrabInteractable>();
        //mask everything so it cant be grabbed
        grabScript.interactionLayerMask = LayerMask.GetMask();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            //enable grabable component
            grabScript.interactionLayerMask = LayerMask.GetMask();

            transform.parent = null;

            transform.position = other.transform.position + other.transform.up * -0.1f;    //move the ball under the goal so it falls out
            this.enabled = false;
        }
    }
}

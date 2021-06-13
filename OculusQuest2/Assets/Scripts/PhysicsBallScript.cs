using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PhysicsBallScript : MonoBehaviour
{
    public Vector3 min, max;


    XRGrabInteractable interactable;



    void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
    }

    void FixedUpdate()
    {

        //lock xyz position relitive to parent
        Vector3 relitivePos = transform.localPosition;
        
        relitivePos = ClampVector(relitivePos);

        //rb.MovePosition(relitivePos);
        transform.localPosition = relitivePos;
    }
    // Clamp a vector between the min and max values
    private Vector3 ClampVector(Vector3 value)
	{
        Vector3 result;
        result.x = Mathf.Clamp(value.x, min.x, max.x);
        result.y = Mathf.Clamp(value.y, min.y, max.y);
        result.z = Mathf.Clamp(value.z, min.z, max.z);
        return result;
	}

    // Called when entering the hole trigger
	void OnTriggerEnter(Collider other)
	{
        // Use default layer
        gameObject.layer = 0;
        // Enable the grab interactable
        interactable.interactionLayerMask = LayerMask.GetMask("Interactable");
        // Remove parent
        transform.parent = null;
        // Stop clamping the position
        this.enabled = false;
	}
}

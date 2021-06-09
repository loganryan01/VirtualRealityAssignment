using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyholeScript : MonoBehaviour
{
    // Where the key should be after being placed
    public Transform keyLockTransform;

    public UnityEvent onUnlocked;

    XRSocketInteractor socket;
    GameObject key;



    void Start()
    {
        // Get the socket and add a listener
        socket = GetComponentInChildren<XRSocketInteractor>();
        socket.onSelectEntered.AddListener(OnKeyPlaced);
    }

    // Called when the key is placed in the socket
    private void OnKeyPlaced(XRBaseInteractable interactable)
	{
        // Disable the socket
        socket.socketActive = false;

        key = interactable.gameObject;

        //dont use gravity, and freeze everything except rotation on the x axis
        Rigidbody rigid = key.GetComponent<Rigidbody>();
        rigid.useGravity = false;
        rigid.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        // Move the key to the correct position
        key.transform.position = keyLockTransform.position;
        key.transform.rotation = keyLockTransform.rotation;

        // Enable the lock rotation script and add listener for when the key is rotated to the end
        LockRotation rotScript = key.GetComponent<LockRotation>();
        rotScript.enabled = true;
        rotScript.onEndAngle.AddListener(OnKeyTurned);
    }

    // Called when the key has been turned
    private void OnKeyTurned()
	{
        // Disable interaction on the key
        key.GetComponent<XRGrabInteractable>().interactionLayerMask = 0;
        Rigidbody rigid = key.GetComponent<Rigidbody>();
        rigid.isKinematic = true;
        rigid.angularVelocity = Vector3.zero;
        key.GetComponent<LockRotation>().enabled = false;

        // Invoke Event
        onUnlocked.Invoke();
	}
}

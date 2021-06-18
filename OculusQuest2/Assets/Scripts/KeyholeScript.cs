using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyholeScript : MonoBehaviour
{
    public XRGrabInteractable gem;

    // Where the key should be after being placed
    public Transform keyLockTransform;
   
    public UnityEvent onUnlocked;
    [Space]
    // Door to open
    public Transform caseDoor;
    // How long it should take to open
    public float time;
    // How much to open it by
    public float angle;

    private XRSocketInteractor socket;
    private GameObject key;



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
        // Freeze everything except rotation on the x axis
        Rigidbody rigid = key.GetComponent<Rigidbody>();
        rigid.useGravity = false;
        rigid.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        // Make the key a trigger so it doesent get stuck inside the case collider
        key.GetComponent<Collider>().isTrigger = true;
        // Move the key to the correct position
        key.transform.position = keyLockTransform.position;
        key.transform.rotation = keyLockTransform.rotation;
        // Enable the lock rotation script and add listener for when the key is rotated to the end
        LockRotation rotScript = key.GetComponent<LockRotation>();
        rotScript.enabled = true;
        rotScript.onEndAngle.AddListener(OnKeyTurned);

        // Set tag to use hand snapping
        key.tag = "Static Grabbable";
        // Destroy the old interactable, and create a new one on the next frame
        Destroy(key.GetComponent<XRGrabInteractable>());
        StartCoroutine(AddNewInteractableOnNextFrame());
    }
    private IEnumerator AddNewInteractableOnNextFrame()
	{
        // Wait one frame
        yield return new WaitForEndOfFrame();

        // Create a non snapping interactable component with the correct attach point
        NonSnapGrabInteractable newInteractable = key.AddComponent<NonSnapGrabInteractable>();
        newInteractable.interactionLayerMask = LayerMask.GetMask("Interactable");
        newInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        newInteractable.attachTransform = key.transform.GetChild(1); //the second child is the hand position
    }


    // Called when the key has been turned
    private void OnKeyTurned()
	{
        // Enable the gem
        gem.interactionLayerMask |= LayerMask.GetMask("Interactable");

        // Disable interaction on the key and stop it from moving
        key.GetComponent<XRGrabInteractable>().interactionLayerMask = 0;
        Rigidbody rigid = key.GetComponent<Rigidbody>();
        rigid.isKinematic = true;
        rigid.constraints = RigidbodyConstraints.FreezeAll;
        key.GetComponent<LockRotation>().enabled = false;

        // Invoke Event
        onUnlocked.Invoke();
        // Open the door
        StartCoroutine(OpenCaseDoor());
    }


    private IEnumerator OpenCaseDoor()
	{
        Quaternion startRot = caseDoor.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);

        float t = 0;
        while (t < time)
		{
            // Rotate the case door over time
            caseDoor.rotation = Quaternion.Slerp(startRot, endRot, t / time);

            t += Time.deltaTime;
            yield return null;
		}
	}
}

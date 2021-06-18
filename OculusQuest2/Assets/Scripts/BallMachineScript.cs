using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BallMachineScript : MonoBehaviour
{
    public XRGrabInteractable interactable;

    public XRSocketInteractor socket;

    public UnityEvent onBallPlaced;

    public Transform door;
    public float openDelay;
    public float openTime;
    public float openAngle;
    


    void Start()
    {
        socket.onSelectEntered.AddListener(OnBallPlaced);
    }

    // Called when the ball is placed in the socket
    private void OnBallPlaced(XRBaseInteractable interactable)
	{
        // Enable the object
        interactable.interactionLayerMask |= LayerMask.GetMask("interactable");

        // Disable the socket and delete the ball
        socket.socketActive = false;
        interactable.gameObject.SetActive(false);

        // Call event
        onBallPlaced.Invoke();


        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
	{
        yield return new WaitForSeconds(openDelay);

        Quaternion start = door.rotation;
        Quaternion end = start * Quaternion.Euler(0, openAngle, 0);

        float t = 0;
        while (t < openTime)
        {
            door.rotation = Quaternion.Slerp(start, end, t / openTime);

            t += Time.deltaTime;
            yield return null;
        }
	}
}

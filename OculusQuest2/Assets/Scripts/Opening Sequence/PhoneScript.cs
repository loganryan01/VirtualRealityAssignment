using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PhoneScript : MonoBehaviour
{
    public GameObject door;
    public GameObject doorLight;

    public float waitTime;

    public AudioSource phoneCallClip;


    XRGrabInteractable interactable;



    void Start()
    {
        interactable = GetComponent<XRGrabInteractable>();
        // Add listener to when the phone is grabbed
        interactable.onSelectEntered.AddListener(OnPickUp);
    }

    // Called when the phone is picked up
    private void OnPickUp(XRBaseInteractor interactor)
	{
        // Play audio clip
        if (phoneCallClip != null)
		{
            phoneCallClip.Play();
        }

        StartCoroutine(WaitDelay());

        // Remove this as a listener
        interactable.onSelectEntered.RemoveListener(OnPickUp);
    }

    private IEnumerator WaitDelay()
	{
        yield return new WaitForSeconds(waitTime);

        // Enable the door
        door.SetActive(true);
        doorLight.SetActive(true);
	}
}

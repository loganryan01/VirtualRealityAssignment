using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PhoneScript : MonoBehaviour
{
    public GameObject door;
    public GameObject doorLight;

    public float waitTime;

    public AudioSource audioSource;
    public AudioClip dialogue;


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
        if (audioSource != null)
		{
            // Set new properties
            audioSource.loop = false;
            audioSource.playOnAwake = false;
            audioSource.Stop();
            // Set the dialogue and play it
            audioSource.clip = dialogue;
            audioSource.Play();
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

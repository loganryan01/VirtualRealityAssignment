using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AudioOnPickup : MonoBehaviour
{
    public AudioSource audioSource;

    [Tooltip("Should audio only be played the first time the item is picked up?")]
    public bool playOnce = false;


    // Has this object already been picked up?
    private bool hasBeenPickedUp = false;



    void Start()
    {
        // Add listener to pickup event
        GetComponent<XRBaseInteractable>().onSelectEntered.AddListener((XRBaseInteractor) => OnPickup());
    }

    // Called when the object is picked up
    private void OnPickup()
    {
        if (!(playOnce && hasBeenPickedUp))
        {
            audioSource.Play();
            hasBeenPickedUp = true;
        }
    }
}

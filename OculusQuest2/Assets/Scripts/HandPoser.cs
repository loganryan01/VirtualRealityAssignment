using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPoser : MonoBehaviour
{
    public InputActionReference indexInputReference;
    public InputActionReference thumbInputReference;
    public InputActionReference gripInputReference;

    private Animator anim;
    private bool isUsingPose = false;
    private bool firstUpdate = true;


    void Start()
    {
        // Add listeners
        XRDirectInteractor interactor = GetComponent<XRDirectInteractor>();
        interactor.onSelectEntered.AddListener(OnGrab);
        interactor.onSelectExited.AddListener(OnDrop);
    }

    void Update()
    {
        // Get the animator on the first update because the hand prefab hasnt been created yet in start
        if (firstUpdate)
        {
            firstUpdate = false;
            anim = GetComponentInChildren<Animator>();
        }

        // The hand is posed so dont use animations
        if (isUsingPose)
        {
            return;
        }

        // Read input and set animator layer weights
        float indexVal = indexInputReference.action.ReadValue<float>();
        float thumbVal = thumbInputReference.action.ReadValue<float>();
        float gripVal = gripInputReference.action.ReadValue<float>();
        anim.SetLayerWeight(1, thumbVal);
        anim.SetLayerWeight(2, indexVal);
        anim.SetLayerWeight(3, gripVal);
        anim.SetLayerWeight(4, gripVal);
        anim.SetLayerWeight(5, gripVal);
    }


    // Called when the controller graps an object
    private void OnGrab(XRBaseInteractable interactable)
    {
        //stop using animations, swap to grab pose
        isUsingPose = true;

        //set pose using data from interactable object

        anim.SetLayerWeight(1, 1);
        anim.SetLayerWeight(2, 1);
        anim.SetLayerWeight(3, 1);
        anim.SetLayerWeight(4, 1);
        anim.SetLayerWeight(5, 1);
    }

    // Called when the controller drops an object
    private void OnDrop(XRBaseInteractable interactable)
    {
        //stop using grab pose, start using animations
        isUsingPose = false;
    }
}

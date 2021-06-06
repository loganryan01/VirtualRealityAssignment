﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class HandPoser : MonoBehaviour
{
    public InputActionReference indexInputReference;
    public InputActionReference thumbInputReference;
    public InputActionReference gripInputReference;
    [Space]
    // Access the GameObject that contains the teleport controller script.
    public GameObject teleportController;
    // Reference to the Input Action Reference that contains the button mapping data for activation.
    public InputActionReference teleportCancelReference;

    public UnityEvent onTeleportActivate;
    public UnityEvent onTeleportCancel;
    [Space]
    public Collider indexEndCollider;
    public Collider indexFingerCollider;


    private Animator anim;
    private bool isUsingPose = false;
    private bool firstUpdate = true;


    void Start()
    {
        // Add listeners
        XRDirectInteractor interactor = GetComponentInChildren<XRDirectInteractor>();
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
        anim.SetLayerWeight(1, gripVal);   //grip controlls thumb
        anim.SetLayerWeight(2, indexVal);
        anim.SetLayerWeight(3, gripVal);
        anim.SetLayerWeight(4, gripVal);
        anim.SetLayerWeight(5, gripVal);


        // If using teleportation and the action has been started
        if (teleportCancelReference.action.phase == InputActionPhase.Started)
        {
            onTeleportActivate.Invoke();
            if (indexEndCollider != null && indexFingerCollider != null)
            {
                indexEndCollider.enabled = false;
                indexFingerCollider.enabled = true;
            }
        }
        else
        {
            Invoke("DelayTeleportationDeactivate", .1f);
        }
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


    private void DelayTeleportationDeactivate()
    {
        onTeleportCancel.Invoke();
        if (indexEndCollider != null && indexFingerCollider != null)
		{
            indexEndCollider.enabled = true;
            indexFingerCollider.enabled = false;
        }
    }
}

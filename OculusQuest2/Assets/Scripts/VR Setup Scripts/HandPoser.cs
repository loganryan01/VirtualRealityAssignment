using System.Collections;
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

    private bool isUsingTeleport;

    public Transform physicalController;
    public Transform handTrans;

    public Transform teleportAttachPoint;
    private Vector3 teleportPointPosition;



    void Start()
    {
        // Add listeners
        XRDirectInteractor interactor = GetComponentInChildren<XRDirectInteractor>();
        interactor.onSelectEntered.AddListener(OnGrab);
        interactor.onSelectExited.AddListener(OnDrop);

        // Save the position on the attach point
        teleportPointPosition = teleportAttachPoint.localPosition;
    }

    void Update()
    {
        // Some things need to be done in the first update call because of race conditions
        if (firstUpdate)
        {
            firstUpdate = false;
            anim = GetComponentInChildren<Animator>();
            // Get movement control type from player prefs script
            isUsingTeleport = PlayerPrefsScript.teleportationMovement;
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

            // When enabling a ray interactor, its attach point gets reset, so set it again
            teleportAttachPoint.localPosition = teleportPointPosition;
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


        if (interactable != null && interactable.CompareTag("Static Grabbable"))
        {
            Transform attachTrans = (interactable as XRGrabInteractable).attachTransform;

            handTrans.parent = attachTrans;
            handTrans.localPosition = Vector3.zero;
            handTrans.localRotation = Quaternion.identity;
            // If the scale is incorrect, calculate the correct local scale
            if (handTrans.lossyScale != Vector3.one)
            {
                Vector3 scale;
                scale.x = 1.0f / attachTrans.lossyScale.x;
                scale.y = 1.0f / attachTrans.lossyScale.y;
                scale.z = 1.0f / attachTrans.lossyScale.z;
                handTrans.localScale = scale;
            }
        }
    }

    // Called when the controller drops an object
    private void OnDrop(XRBaseInteractable interactable)
    {
        //stop using grab pose, start using animations
        isUsingPose = false;


        if (interactable != null && interactable.CompareTag("Static Grabbable"))
        {
            handTrans.parent = physicalController;
            handTrans.localPosition = Vector3.zero;
            handTrans.localRotation = Quaternion.identity;
            handTrans.localScale = Vector3.one;
        }
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

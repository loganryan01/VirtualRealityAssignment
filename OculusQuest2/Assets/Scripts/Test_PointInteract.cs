using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Test_PointInteract : MonoBehaviour
{
    public InputActionReference pointActivateReference;

    public SphereCollider interactCol;


    private float oldRadius;
    private Vector3 oldOffset;


    void Start()
    {
        pointActivateReference.action.performed += PointModeActivate;
        pointActivateReference.action.canceled += PointModeCancel;

        oldRadius = interactCol.radius;
        oldOffset = interactCol.center;
    }


    private void PointModeActivate(InputAction.CallbackContext obj)
    {
        interactCol.radius = 0.1f;
        interactCol.center = new Vector3(0.5f, 0, 0);
    }

    private void PointModeCancel(InputAction.CallbackContext obj)
    {
        interactCol.radius = oldRadius;
        interactCol.center = oldOffset;
    }
}

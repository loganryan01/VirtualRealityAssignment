using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class HandButton : MonoBehaviour
{
    private bool pressed = false;

    public Vector3 endPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (!pressed && other.gameObject.name == "Index")
        {
            pressed = true;

            transform.localPosition = endPosition;
        }
    }
}

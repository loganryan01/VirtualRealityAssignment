using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LockRotation : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    [Tooltip("The axis the object should rotate on")]
    public Axis rotationAxis;

    [Tooltip("The start angle cap")]
    public float startAngle;
    [Tooltip("The end angle cap")]
    public float endAngle;

    public UnityEvent onStartAngle;
    public UnityEvent onEndAngle;


    private Vector3 axis;
    private Vector3 zeroDirection;
    private float halfAngleDiff;
    private Quaternion initialRotation;



    void Start()
    {
        halfAngleDiff = (endAngle - startAngle) * 0.5f;
        initialRotation = transform.rotation;

        // Set direction vectors according to selected axis
        switch (rotationAxis)
        {
            case Axis.X:
                axis = Vector3.right;
                zeroDirection = Vector3.up;
                break;
            case Axis.Y:
                axis = Vector3.up;
                zeroDirection = Vector3.forward;
                break;
            case Axis.Z:
                axis = Vector3.forward;
                zeroDirection = Vector3.up;
                break;
        }
    }

    void FixedUpdate()
    {
        // Get angle, looping with account to the range
        float angle = GetRotation();
        if (angle > endAngle + halfAngleDiff)
		{
            angle -= 360;
		}

        if (angle <= startAngle)
		{
            Vector3 rot = axis * startAngle;
            transform.rotation = initialRotation * Quaternion.Euler(rot);
        }
        else if (angle >= endAngle)
		{
            Vector3 rot = axis * endAngle;
            transform.rotation = initialRotation * Quaternion.Euler(rot);
        }
    }

	void Update()
	{
        // Get angle, looping with account to the range
        float angle = GetRotation();
        if (angle > endAngle + halfAngleDiff)
        {
            angle -= 360;
        }

        // Invoke events if we are at the start or end angles
        if (angle <= startAngle)
		{
            onStartAngle.Invoke();
		}
        else if (angle >= endAngle)
		{
            onEndAngle.Invoke();
        }
	}


	// Get this objects rotation along the rotation axis
	public float GetRotation()
    {
        // Find the 'forward' direction by the selected axis
        Vector3 forward;
        switch (rotationAxis)
        {
            case Axis.X:
                forward = transform.up;
                break;
            case Axis.Y:
                forward = transform.forward;
                break;
            case Axis.Z:
                forward = transform.up;
                break;

            default:
                forward = Vector3.up;
                break;
        }

        forward = initialRotation * forward;

        // Get the angle, looping it so 0 is at zeroDirection and going clockwise
        float angle = Vector3.SignedAngle(zeroDirection, forward, axis);
        if (angle < 0)
        {
            angle += 360;
        }

        return angle;
    }
}

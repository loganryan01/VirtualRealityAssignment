using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SnapRotate : MonoBehaviour
{
    public enum Axis
	{
        X,
        Y,
        Z
	}

    [Tooltip("The axis the object should rotate on")]
    public Axis rotationAxis;

    [Tooltip("How many stable points there should be")]
    public int divisions;
    [Tooltip("Multiplies the amount that the object is rotated")]
    public float correctionStrength = 5;
    [Tooltip("Multiplies the amount of conter torque to avoid over rotation")]
    public float counterTorqueStrength = 2;


    private Vector3 axis;
    private Vector3 zeroDirection;
    private float divisionSize;
    private Rigidbody rb;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        rb.ResetInertiaTensor();
        rb.inertiaTensorRotation = Quaternion.identity;

        divisionSize = 360.0f / divisions;

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

        // Get the current rotation along the axis
        float angle = Vector3.SignedAngle(zeroDirection, forward, axis) + 180 + divisionSize * 0.5f;
        // Find the amount that the hand is off from a stable point as a scalar
        float diff = divisionSize * 0.5f - (angle % divisionSize);
        diff /= (divisionSize * 0.5f);
        // Use the angular velocity along the rotation axis to prevent over rotating
        float counterTorque = Vector3.Dot(rb.angularVelocity, axis) * counterTorqueStrength * (1.2f - diff);

        // Calculate and apply torque
        Vector3 torque = axis * (diff * correctionStrength - counterTorque);
        rb.AddTorque(torque * Time.fixedDeltaTime);
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

        // Get the angle, looping it so 0 is at zeroDirection and going clockwise
        float angle = Vector3.SignedAngle(zeroDirection, forward, axis);
        if (angle < 0)
		{
            angle += 360;
		}

        return angle;
    }
}

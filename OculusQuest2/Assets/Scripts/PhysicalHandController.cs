using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicalHandController : MonoBehaviour
{
    // The transform that is tracking the controllers position and rotation
    public Transform trackingController;
    [Space]
    public float positionStrength = 15;
    public float maxDistance = 0.5f;
    public float minDistance = 0.005f;
    [Space]
    public float rotationStrength = 30;
    public float minRotationDiff = 10f;
    
    private Rigidbody rb;


	void Awake()
	{
        rb = GetComponent<Rigidbody>();
        // Set the center of mass so torque works as expected
        rb.centerOfMass = transform.localPosition;
    }

    void FixedUpdate()
    {
        float dist = Vector3.Distance(trackingController.position, rb.position);
        // If the distance is outside the smooth range, set the position
        if (dist > maxDistance || dist < minDistance)
        {
            rb.MovePosition(trackingController.position);
        }
        else
        {
            // Set the velocity to the direction of the tracked position, scaled by the distance
            rb.velocity = (trackingController.position - rb.position).normalized * dist * positionStrength;
        }


        // If the difference in angle is less than min, set the rotation
        if (Quaternion.Angle(rb.rotation, trackingController.rotation) < minRotationDiff)
        {
            rb.MoveRotation(trackingController.rotation);
        }
        else
        {
            // Constants relating to rotation strength. Could be calculated in awake, but leave here so strength can be changed in play mode
            float rotConst1 = (6 * rotationStrength) * (6 * rotationStrength) * 0.25f;
            float rotConst2 = 4.5f * rotationStrength;

            // Get the tracked rotation relitive to this, then conver it to axis angle
            Quaternion localTrackedRot = trackingController.rotation * Quaternion.Inverse(transform.rotation);
            localTrackedRot.ToAngleAxis(out float xMag, out Vector3 x);
            x = x.normalized * Mathf.Deg2Rad;

            // Find the change in angular momentum
            Vector3 torque = rotConst1 * x * xMag - rotConst2 * rb.angularVelocity;
            // Transform by inertia tensor
            Quaternion rotInertiaToWorld = rb.inertiaTensorRotation * transform.rotation;
            torque = Quaternion.Inverse(rotInertiaToWorld) * torque;
            torque.Scale(rb.inertiaTensor);
            torque = rotInertiaToWorld * torque;

            // Apply torque
            rb.AddTorque(torque);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ClockHandMovement : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    [Tooltip("The axis the object should rotate on")]
    public Axis rotationAxis;

    public int numberOfPoints = 12;
    public float correctionStrength = 5;
    public float counterTorqueStrength = 2;
    
    public Transform smallHand;
    // The correct time to be entered
    public float puzzleMinute;
    public float puzzleHour;
    // How long to wait for an answer to be checked
    public float puzzleTimmer;
    public float puzzleThreshold = 0.1f;
    public UnityEvent onPuzzleComplete;

    public Transform doorTransform;
    public float doorTime;
    public float doorAngle;


    Vector3 axis;
    Vector3 zeroDirection;

    float divisionSize;
    Rigidbody rb;
    // Public for custom editor script
    public float lastRot;
    public float smallHandRot;

    float timmer = 0;



    void Start()
    {
        divisionSize = 360 / numberOfPoints;

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.zero;
        rb.ResetInertiaTensor();

        lastRot = transform.eulerAngles.x;

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
        Vector3 bigHandForward, smallHandForward;
        switch (rotationAxis)
        {
            case Axis.X:
                bigHandForward = transform.up;
                smallHandForward = smallHand.up;
                break;
            case Axis.Y:
                bigHandForward = transform.forward;
                smallHandForward = smallHand.forward;
                break;
            case Axis.Z:
                bigHandForward = transform.up;
                smallHandForward = smallHand.up;
                break;

            default:
                bigHandForward = Vector3.up;
                smallHandForward = Vector3.up;
                break;
        }


        // ---------- Big Hand ----------
        // Get the current rotation along the axis
        float angle = Vector3.SignedAngle(zeroDirection, bigHandForward, axis) + 180 + divisionSize * 0.5f;
        // Find the amount that the hand is off from a stable point as a scalar
        float diff = divisionSize * 0.5f - (angle % divisionSize);
        diff /= (divisionSize * 0.5f);
        // Use the angular velocity along the rotation axis to prevent over rotating
        float counterTorque = Vector3.Dot(rb.angularVelocity, axis) * counterTorqueStrength * (1.2f - diff);

        // Calculate and apply torque
        Vector3 torque = axis * (diff * correctionStrength - counterTorque);
        rb.AddTorque(torque * Time.fixedDeltaTime);


        // ---------- Small Hand ----------
        float rotDiff = angle - lastRot;
        // Loop over-rotation
        if (rotDiff > 180)
        {
            rotDiff -= 360;
        }
        if (rotDiff < -180)
        {
            rotDiff += 360;
        }

        // Get the rotation of the small hand
        smallHandRot = Vector3.SignedAngle(zeroDirection, smallHandForward, axis);
        // Convert the big hand difference to hours and add it to the small hand
        smallHandRot += rotDiff / 12;
        // Rotate the small hand
        smallHand.eulerAngles = Vector3.zero + (axis * smallHandRot );


        // Update last big hand rotation
        lastRot = angle;


        // ---------- Puzzle ----------
        if (Mathf.Abs(diff) < 0.2f)
        {
            timmer += Time.deltaTime;
            if (timmer >= puzzleTimmer)
            {
                // Check if the input is the answer
                if (puzzleMinute >= lastRot - puzzleThreshold && puzzleMinute <= lastRot + puzzleThreshold && 
                    puzzleHour >= smallHandRot - puzzleThreshold && puzzleHour <= smallHandRot + puzzleThreshold)
                {
                    // Correct answer, call event and disable clock
                    onPuzzleComplete.Invoke();
                    StartCoroutine(OpenDoor());
                    rb.isKinematic = true;
                    this.enabled = false;
                }
            }
        }
        else
        {
            // Reset the timmer
            timmer = 0;
        }
    }

    private IEnumerator OpenDoor()
	{
        Quaternion start = doorTransform.rotation;
        Quaternion end = doorTransform.rotation * Quaternion.Euler(0, doorAngle, 0);

        float t = 0;
        while (t < doorTime)
		{
            doorTransform.rotation = Quaternion.Slerp(start, end, t / doorTime);

            t += Time.deltaTime;
            yield return null;
		}
    }
}

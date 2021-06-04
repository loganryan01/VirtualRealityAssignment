using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class ClockHandMovement : MonoBehaviour
{
    public int numberOfPoints = 12;
    public Vector3 rotationAxis;
    public float correctionStrength = 5;
    public Transform smallHand;
    // The correct time to be entered
    public float puzzleMinute;
    public float puzzleHour;
    // How long to wait for an answer to be checked
    public float puzzleTimmer;
    public float puzzleThreshold = 0.1f;
    public UnityEvent onPuzzleComplete;


    float divisionSize;
    Rigidbody rb;
    // Public for custom editor script
    public float lastRot;
    public float smallHandRot;

    float timmer = 0;



    void Start()
    {
        rotationAxis.Normalize();

        divisionSize = 360 / numberOfPoints;

        rb = GetComponent<Rigidbody>();

        lastRot = transform.eulerAngles.x;
    }

    void FixedUpdate()
    {
        // ---------- Big Hand ----------
        Vector3 currentRot = transform.eulerAngles;
        Vector3 localUp = transform.up;

        // Get the current rotation along the axis
        float angle = Vector3.SignedAngle(Vector3.up, localUp, rotationAxis) + 180 + divisionSize * 0.5f;

        // Find the amount that the hand is off from a stable point
        float diff = divisionSize * 0.5f - (angle % divisionSize);

        // Flip the direction sometimes, I dont know. This will probably break if the object is not rotating on the x axis
        if (currentRot.y == 180 || (currentRot.z == 180 && currentRot.x > 270) || (currentRot.z == 180 && currentRot.x < 90))
        {
            diff *= -1;
        }

        // Find the change in rotation, and the new rotation
        Vector3 torque = rotationAxis * diff * correctionStrength * Time.deltaTime;
        Vector3 newRot = currentRot + torque;
        rb.MoveRotation(Quaternion.Euler(newRot));


        // ---------- Small Hand ----------
        float rotDiff = angle - lastRot;
        Vector3 smallRot = smallHand.eulerAngles;

        // IDK, it works
        if (smallRot.y == 180 || (smallRot.z == 180 && smallRot.x > 270) || (smallRot.z == 180 && smallRot.x < 90))
        {
            rotDiff *= -1;
        }
        // Loop over-rotation
        if (rotDiff > 180)
        {
            rotDiff -= 360;
        }
        if (rotDiff < -180)
        {
            rotDiff += 360;
        }

        // Convert to hours for small hand
        smallRot.x += rotDiff / 12;
        smallHand.eulerAngles = smallRot;
        // Get the current rotation of the small hand
        smallHandRot = Vector3.SignedAngle(Vector3.up, smallHand.up, rotationAxis) + 180;

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
}

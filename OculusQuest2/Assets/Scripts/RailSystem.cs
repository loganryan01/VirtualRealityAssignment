using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RailSystem : MonoBehaviour
{
    [System.Serializable]
    public struct RailSegment
    {
        public Vector3 start;
        public Vector3 end;

        public bool isStraight;    //false means curved

        public Vector3 curvePoint;

        public Vector3 startRotation;
        public Vector3 endRotation;


        [HideInInspector]
        public float sqrLength;
    }

    public RailSegment[] rails;
    [HideInInspector]
    public uint currentRail = 0;

    public UnityEvent onStartOfRail;
    public UnityEvent onEndOfRail;


    //for editor
    [HideInInspector]
    public float totalDist = 0;     //dist along all rails
    [HideInInspector]
    public float currentDist = 0;   //dist along current rail
    public Color color = Color.red;   //color to draw rails with



    void Start()
    {
        // Calculate the length of each rail segment
        for (int i = 0; i < rails.Length; i++)
        {
            rails[i].sqrLength = Vector3.SqrMagnitude(rails[i].start - rails[i].end);
        }
    }

    void Update()
    {
        RailSegment rail = rails[currentRail];
        Vector3 pos = transform.position;


        Vector3 allignedPos;
        float distanceAlong;
        if (rail.isStraight)
        {
            // Find the closest point on the line
            allignedPos = rail.start + Vector3.Project(pos - rail.start, rail.end - rail.start);
            // Find how far along the line the point is
            distanceAlong = (Vector3.SqrMagnitude(allignedPos - rail.start) - Vector3.SqrMagnitude(allignedPos - rail.end)) / rail.sqrLength;
            distanceAlong = (distanceAlong + 1) * 0.5f;
        }
        else
        {
            // Use bezier curve
            distanceAlong = GetClosestT(pos, rail.start, rail.curvePoint, rail.end);
            allignedPos = GetPointOnCurve(distanceAlong, rail.start, rail.curvePoint, rail.end);
        }

        Quaternion currentRot = Quaternion.Lerp(Quaternion.Euler(rail.startRotation), Quaternion.Euler(rail.endRotation), distanceAlong);


        // We are past the start of the rail
        if (distanceAlong <= 0.001f)
        {
            allignedPos = rail.start;
            currentRot = Quaternion.Euler(rail.startRotation);

            if (currentRail > 0)
            {
                currentRail--;
            }
            // We are at the start of the first rail
            else
            {
                onStartOfRail.Invoke();
            }
        }
        // We are past the end of the rail
        else if (distanceAlong >= 0.999f)
        {
            allignedPos = rail.end;
            currentRot = Quaternion.Euler(rail.endRotation);

            if (currentRail < rails.Length - 1)
            {
                currentRail++;
            }
            // We are at the end of the last rail
            else
            {
                onEndOfRail.Invoke();
            }
        }
        
        // Keep the object on the rail
        transform.position = allignedPos;
        transform.rotation = currentRot;


        // Set values for the editor
        currentDist = distanceAlong;
        // Calculate the total distance along the rail the object is
        float preDist = 0, postDist = 0;
        for (int i = 0; i < rails.Length; i++)
        {
            if (i == currentRail)
            {
                preDist += rails[i].sqrLength * distanceAlong;
                postDist += rails[i].sqrLength * (1 - distanceAlong);
            }
            else if (i < currentRail)
            {
                preDist += rails[i].sqrLength;
            }
            else
            {
                postDist += rails[i].sqrLength;
            }
        }
        totalDist = (preDist - postDist) / (preDist + postDist);
        totalDist = (totalDist + 1) * 0.5f;
    }




    private Vector3 GetPointOnCurve(float t, in Vector3 p1, in Vector3 p2, in Vector3 p3)
    {
        if (t < 0 || t > 1)
        {
            return Vector3.zero;
        }

        float c = 1.0f - t;

        // The Bernstein polynomials
        float bb0 = c * c;
        float bb1 = 2 * c * t;
        float bb2 = t * t;

        // Return the point
        return p1 * bb0 + p2 * bb1 + p3 * bb2;
    }


    // Find the t value on a curve closest to a given point
    private float GetClosestT(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        const float threshold = 0.0005f;

        // Use a recursive lambda function to find t
        Func<float, float, float> recursiveLoop = null;
        recursiveLoop = (float start, float end) =>
        {
            float mid = (start + end) * 0.5f;

            // If the value is within the threshold, it is t
            if ((end - start) < threshold)
            {
                return mid;
            }

            // Use a binary search to find the closest point
            float paramA = (start + mid) * 0.5f;
            float paramB = (mid + end) * 0.5f;

            Vector3 posA = GetPointOnCurve(paramA, p1, p2, p3);
            Vector3 posB = GetPointOnCurve(paramB, p1, p2, p3);
            float distASqr = (posA - point).sqrMagnitude;
            float distBSqr = (posB - point).sqrMagnitude;

            if (distASqr < distBSqr)
            {
                end = mid;
            }
            else
            {
                start = mid;
            }

            // Recursive call
            return recursiveLoop(start, end);
        };


        return recursiveLoop(0, 1);
    }
}

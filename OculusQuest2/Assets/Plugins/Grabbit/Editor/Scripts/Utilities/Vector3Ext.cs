#if UNITY_EDITOR


using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grabbit
{
    public static class Vector3Ext
    {
        public static Vector3 GetClampedMagnitude(this Vector3 vec, float minMagnitude, float maxMagnitude)
        {
            if (vec.magnitude < minMagnitude)
                return vec.normalized * minMagnitude;

            return Vector3.ClampMagnitude(vec, maxMagnitude);
        }
        
        public static Vector3 Mult(this Vector3 one, Vector3 two)
        {
            return new Vector3(one.x * two.x, one.y * two.y, one.z * two.z);
        }

    }
}
#endif
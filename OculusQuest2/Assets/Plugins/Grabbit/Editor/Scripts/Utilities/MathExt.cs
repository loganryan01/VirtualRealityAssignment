#if UNITY_EDITOR


using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grabbit
{
    public static class MathExt
    {
        
        public static float Percent(float from, float to, float value)
        {
            return (value - from) / (to - from);
        }
      
    }
    
}
#endif
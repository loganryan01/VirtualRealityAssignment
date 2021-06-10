#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grabbit
{
    public static class ListExt
    {

        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        
        public static IEnumerable<T> Convert<T, T1>(this IEnumerable<T1> en, Func<T1, T> conversion)
        {
            var newList = new List<T>();

            foreach (var member in en) newList.Add(conversion(member));
            return newList;
        }

    }
}
#endif
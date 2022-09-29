using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
    public static bool IsEmpty<T>(this List<T> list)
    {
        return list.Count == 0;
    }

    public static void Trim<T>(this List<T> list, int indexToRemoveFromUntilEnd)
    {
        Debug.Assert(indexToRemoveFromUntilEnd < list.Count);
        list.RemoveRange(indexToRemoveFromUntilEnd, list.Count - indexToRemoveFromUntilEnd);
    }
}

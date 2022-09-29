using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringExtensions
{
    public static int CountOf(this string str, string find)
    {
        int count = 0;
        int lastIndex = -1;

        bool CanContinue()
        {
            if (lastIndex != -1)
            {
                ++count;
                return true;
            }

            return false;
        }

        do
        {
            lastIndex = str.IndexOf(find, lastIndex + 1, StringComparison.Ordinal);
        } while (CanContinue());
        
        return count;
    }

    public static string GetRange(this string str, int start, int end)
    {
        Debug.Assert(start >= 0 && start < str.Length && start < end);
        Debug.Assert(end > start && end <= str.Length);

        return str.Substring(start, end - start);
    }

    // Shave characters off of each end of the string.
    //  e.g. GetTrimmed(string, 1) would return trin, GetTrimmed(string, 2) would return ri
    public static string GetTrimmed(this string str, int factor)
    {
        Debug.Assert(str.Length > factor * 2);
        Debug.Assert(factor >= 1);

        return GetRange(str, factor, str.Length - factor);
    }

    public static string GetTrimmedOffEnd(this string str, int numToTakeOffEnd)
    {
        return GetRange(str, 0, str.Length - numToTakeOffEnd);
    }

    public static void AddRange(this StringBuilder sb, List<string> range)
    {
        foreach (var str in range)
        {
            sb.Append(str);
        }
    }
}

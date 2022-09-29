using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextAssetExtensionMethods
{
    public static List<string> ToList(this TextAsset textAsset)
    {
        List<string> lines = new List<string>(textAsset.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries));

        for (int i = 0; i < lines.Count; ++i)
        {
            lines[i] = lines[i].Replace("\t", "");
        }
        
        return lines;
    }
}

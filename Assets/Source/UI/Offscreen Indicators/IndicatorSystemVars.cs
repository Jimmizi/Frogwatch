using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorSystemVars : ServiceVars
{
    [Tooltip("A list of all objects that get tracked and display offscreen indicators")]
    public List<IndicatorTracker> trackedObjects = new();

    [Tooltip("A parent object to keep offscreen indicators organised under (if not set becomes this object)")]
    public Transform indicatorHolder;

    private void Awake()
    {
        if (!indicatorHolder)
        {
            indicatorHolder = transform;
        }
    }
}

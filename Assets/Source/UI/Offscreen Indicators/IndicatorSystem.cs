using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorSystem : SystemObjectWithVars<IndicatorSystemVars>
{
    public override void AwakeService()
    {
        //throw new System.NotImplementedException();
    }

    public override void FixedUpdateService()
    {
        //throw new System.NotImplementedException();
    }

    public override void StartService()
    {
        //throw new System.NotImplementedException();
    }

    public override void UpdateService()
    {
        //throw new System.NotImplementedException();
    }

    public void AddTrackedObject(IndicatorTracker trackedObject)
    {
        GetVars().trackedObjects.Add(trackedObject);

        if (trackedObject.indicatorUIPrefab)
        {
            trackedObject.indicatorUI = Object.Instantiate(trackedObject.indicatorUIPrefab);
            trackedObject.indicatorUI.trackedObject = trackedObject;
            trackedObject.indicatorUI.transform.parent = GetVars().indicatorHolder;
        }
    }

    public void RemoveTrackedObject(IndicatorTracker trackedObject)
    {
        GetVars().trackedObjects.Remove(trackedObject);

        if (trackedObject.indicatorUI)
        {
            Object.Destroy(trackedObject.indicatorUI.gameObject);
        }
    }
}

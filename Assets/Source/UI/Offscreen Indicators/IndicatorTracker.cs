using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorTracker : MonoBehaviour
{
    [Tooltip("The indicator UI prefab to show for this object")]
    public IndicatorBaseUI indicatorUIPrefab;

    /// <summary> Reference to the indicator UI (created at runtime by the IndicatorSystem) </summary>
    public IndicatorBaseUI indicatorUI { get; set; }


    private void Awake()
    {
        Service.Get<IndicatorSystem>().AddTrackedObject(this);
    }

    private void OnDestroy()
    {
        Service.Get<IndicatorSystem>().RemoveTrackedObject(this);
    }
}

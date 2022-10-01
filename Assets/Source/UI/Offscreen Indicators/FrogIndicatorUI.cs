using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogIndicatorUI : IndicatorBaseUI
{
    private FrogController _frogController;

    /// <summary>
    /// Returns a reference to the frog controller component
    /// </summary>
    public FrogController frogController
    {
        get
        {
            if (_frogController == null)
            {
                _frogController = trackedObject?.GetComponent<FrogController>();
            }

            return _frogController;
        }
    }

    /// <summary>
    /// Returns the state of the frog
    /// </summary>
    public FrogController.State state
    {
        get
        {
            return frogController?.GetState() ?? FrogController.State.Idle;
        }
    }

    // Override isVisible, the frog indicator becomes invisible when the frog is held by a witch
    // If an enemy witch is holding it the enemy witch indicator changes to indicate it's kidnapped the frog
    public override bool isVisible
    {
        get
        {
            bool isCarried = (state == FrogController.State.Carried);
            return base.isVisible && !isCarried;
        }
    }
}

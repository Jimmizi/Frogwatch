using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WitchIndicatorUI : IndicatorBaseUI
{
    [SerializeField]
    private Image _exclamationIcon;

    [SerializeField]
    private Animator _animator;

    private EnemyController _enemyController;

    /// <summary>
    /// Returns a reference to the enemy controller component (the witch controller)
    /// </summary>
    public EnemyController enemyController
    {
        get
        {
            if (_enemyController == null)
            {
                _enemyController = trackedObject?.GetComponent<EnemyController>();
            }

            return _enemyController;
        }
    }

    /// <summary>
    /// Returns the state of the witch
    /// </summary>
    public EnemyController.State state
    {
        get
        {
            return enemyController?.GetState() ?? EnemyController.State.Idle;
        }
    }

    protected override void Update()
    {
        base.Update();

        if (_exclamationIcon)
        {
            _exclamationIcon.enabled = isVisible;
        }

        if (_animator)
        {
            bool hasFrog = (state == EnemyController.State.Fleeing);
            _animator.SetBool("HasFrog", hasFrog);
        }
    }
}

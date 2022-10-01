using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashSolver : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!HumanoidController.Player.IsDashing)
        {
            return;
        }

        if (col.tag != "DashCollider")
        {
            return;
        }
        
        var enemyControl = col.gameObject.GetComponentInParent<EnemyController>();
        if (enemyControl)
        {
            Debug.Log($"Dashed into {col.gameObject.name}");
            enemyControl.SetDashedInto();
        }
    }
}

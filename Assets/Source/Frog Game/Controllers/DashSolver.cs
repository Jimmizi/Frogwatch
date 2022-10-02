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
        TryCollide(col);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        TryCollide(col);
    }

    void OnTriggerStay2D(Collider2D col)
    {
        TryCollide(col);
    }

    void TryCollide(Collider2D col)
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
            enemyControl.SetDashedInto();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogManagerSystem : SystemObjectWithVars<FrogSystemVars>
{
    private float cacheAvgFrogPosTimer;
    public Vector2 averageFrogPosition;



    public override void AwakeService()
    {
        
    }

    public override void StartService()
    {
        
    }

    public override void UpdateService()
    {
        cacheAvgFrogPosTimer -= Time.deltaTime;
        if (cacheAvgFrogPosTimer <= 0.0f)
        {
            cacheAvgFrogPosTimer = 1.5f;
            CalculateAverageFrogPosition();
        }
    }

    public override void FixedUpdateService()
    {
        
    }

    void CalculateAverageFrogPosition()
    {
        if (FrogController.FrogList.Count == 0)
        {
            averageFrogPosition = Vector2.zero;
            return;
        }

        Vector3 vTotalPosition = Vector2.zero;

        foreach (var frog in FrogController.FrogList)
        {
            vTotalPosition += frog.transform.position;
        }
        
        averageFrogPosition = vTotalPosition / FrogController.FrogList.Count;
    }
    
    public override void OnDrawGizmos()
    {
        //Gizmos.color = Color.gray;
        //Gizmos.DrawSphere(averageFrogPosition, 0.25f);
    }
}

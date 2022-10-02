using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogManagerSystem : SystemObjectWithVars<FrogSystemVars>
{
    private float cacheAvgFrogPosTimer;
    public Vector2 averageFrogPosition;

    public void StartSpawningFrogs(bool bFirstSpawnIsInstant)
    {
        isSpawningFrogs = true;
        frogSpawnTimer = bFirstSpawnIsInstant ? 0.0f : GetTimeBetweenSpawns();
    }
    public void StopSpawningFrogs()
    {
        isSpawningFrogs = false;
    }

    private bool isSpawningFrogs = false;
    private float frogSpawnTimer;

    float GetTimeBetweenSpawns()
    {
        return 10.0f;
    }

    public override void AwakeService()
    {
        
    }

    public override void StartService()
    {
       // StartSpawningFrogs(true);
    }

    public override void UpdateService()
    {
        cacheAvgFrogPosTimer -= Time.deltaTime;
        if (cacheAvgFrogPosTimer <= 0.0f)
        {
            cacheAvgFrogPosTimer = 1.5f;
            CalculateAverageFrogPosition();
        }

        if (isSpawningFrogs)
        {
            frogSpawnTimer -= Time.deltaTime;
            TrySpawnFrog();
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


    void TrySpawnFrog()
    {
        Vector2 vSpawnPos = GetFrogSpawnPosition();

        //

        GameObject newFrog = Object.Instantiate(Service.Vars<FrogSystemVars>().FrogPrefab);
        FrogController controller = newFrog.GetComponent<FrogController>();
        controller.ExternalSetPosition(vSpawnPos);
        controller.OnJustSpawned();
    }

    Vector2 GetFrogSpawnPosition()
    {
        return Vector2.zero;
    }

    public override void OnDrawGizmos()
    {
        //Gizmos.color = Color.gray;
        //Gizmos.DrawSphere(averageFrogPosition, 0.25f);
    }
}

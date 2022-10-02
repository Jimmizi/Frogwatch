using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        StartSpawningFrogs(true);
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

            if (frogSpawnTimer <= 0.0f)
            {
                frogSpawnTimer = GetTimeBetweenSpawns();
                TrySpawnFrog();
            }
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
        
        GameObject newFrog = Object.Instantiate(Service.Vars<FrogSystemVars>().FrogPrefab);
        FrogController controller = newFrog.GetComponent<FrogController>();
        controller.ExternalSetPosition(vSpawnPos);
        controller.OnJustSpawned();
    }

    Vector2 GetFrogSpawnPosition()
    {
        BoxCollider2D bounds = GetVars().FrogMovementBounds;
        
        SortedDictionary<float, Vector2> vRandomPoints = new();
        List<Vector2> vPerfectPoints = new();

        Vector2 vWorldPos = new Vector2(bounds.transform.position.x, bounds.transform.position.y) + bounds.offset;
        float halfHori = (bounds.size.x / 2) * 0.8f;
        float halfVert = (bounds.size.y / 2) * 0.8f;

        float fAvoidRad = GetVars().SpawnNearbyAvoidRadius;

        // Expensive but whatever

        for (int i = 0; i < 15; ++i)
        {
            Vector2 vRandomWorldPos = vWorldPos + new Vector2(Random.Range(-halfHori, halfHori), Random.Range(-halfVert, halfVert));

            if (!bounds.OverlapPoint(vRandomWorldPos))
            {
                continue;
            }

            if (StaticObject.GetOverlapped(vRandomWorldPos) != null)
            {
                continue;
            }

            if (PondDropArea.GetOverlapped(vRandomWorldPos) != null)
            {
                continue;
            }

            // Higher scores means further away from things

            float fTotalDistNearby = 0.0f;
            List<HumanoidController> nearbyEnts = HumanoidController.GetControllersInArea(vRandomWorldPos, fAvoidRad);
            foreach (var ent in nearbyEnts)
            {
                bool isFrog = ent.ZSort.IsFrog;
                float dist = Vector2.Distance(vRandomWorldPos, ent.GetOffsetPosition());

                // Slightly prefer frogs
                fTotalDistNearby += (isFrog ? dist * 0.75f : dist);
            }

            if (nearbyEnts.Count > 0)
            {
                fTotalDistNearby /= nearbyEnts.Count;
            }
            else
            {
                // Nothing nearby, give a perfect score
                fTotalDistNearby = 1.0f;

                vPerfectPoints.Add(vRandomWorldPos);
                continue;
            }

            if (!vRandomPoints.ContainsKey(fTotalDistNearby))
            {
                vRandomPoints.Add(fTotalDistNearby, vRandomWorldPos);
            }
        }

        if (vPerfectPoints.Count > 0)
        {
            return vPerfectPoints[Random.Range(0, vPerfectPoints.Count)];
        }

        if (vRandomPoints.Count > 0)
        {
            return vRandomPoints.ElementAt(Random.Range(0, vRandomPoints.Count)).Value;
        }

        return Vector2.zero;
    }

    public override void OnDrawGizmos()
    {
        //Gizmos.color = Color.gray;
        //Gizmos.DrawSphere(averageFrogPosition, 0.25f);
    }
}

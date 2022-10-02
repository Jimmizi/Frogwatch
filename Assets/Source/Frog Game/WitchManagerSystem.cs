using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WitchManagerSystem : SystemObject
{
    public void StartSpawningWitches(int iTargetNumber)
    {
        isSpawningWitches = true;
        targetNumberOfWitches = iTargetNumber;
        spawnTimer = GetTimeBetweenSpawns();
    }
    public void StopSpawningFrogs()
    {
        isSpawningWitches = false;
    }

    public void UpdateTargetCount(int iCount)
    {
        targetNumberOfWitches = iCount;
    }

    private bool isSpawningWitches = false;
    private float spawnTimer;
    private int targetNumberOfWitches;

    float GetTimeBetweenSpawns()
    {
        return 10.0f;
    }

    public override void AwakeService()
    {

    }

    public override void StartService()
    {
        StartSpawningWitches(1);
        spawnTimer = 1.0f;
    }

    public override void UpdateService()
    {
        if (isSpawningWitches)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0.0f)
            {
                spawnTimer = GetTimeBetweenSpawns();
                TrySpawnWitch();
            }
        }
    }

    public override void FixedUpdateService()
    {

    }

    void TrySpawnWitch()
    {
        Vector2 vSpawnPos = GetSpawnPosition();

        GameObject witch = Object.Instantiate(Service.Vars<FrogSystemVars>().WitchPrefab);
        EnemyController controller = witch.GetComponent<EnemyController>();
        controller.ExternalSetPosition(vSpawnPos);
        controller.OnJustSpawned();
    }
    
    Vector2 GetSpawnPosition()
    {
        BoxCollider2D bounds = Service.Vars<FrogSystemVars>().FrogMovementBounds;

        SortedDictionary<float, Vector2> vRandomPoints = new();

        Vector2 vWorldPos = new Vector2(bounds.transform.position.x, bounds.transform.position.y) + bounds.offset;
        float halfHori = (bounds.size.x / 2) * 0.9f;
        float halfVert = (bounds.size.y / 2) * 0.9f;

        float fAvoidRad = 10.0f;
        Vector2 vPlayerPos = HumanoidController.Player.GetOffsetPosition();

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
            
            float fDistToPlayer = Vector2.Distance(vPlayerPos, vRandomWorldPos);
            if (!vRandomPoints.ContainsKey(fDistToPlayer))
            {
                vRandomPoints.Add(fDistToPlayer, vRandomWorldPos);
            }
        }
        
        if (vRandomPoints.Count > 5)
        {
            return vRandomPoints.ElementAt(Random.Range(vRandomPoints.Count - 3, vRandomPoints.Count)).Value;
        }
        else if (vRandomPoints.Count > 0)
        {
            return vRandomPoints.Last().Value;
        }

        return Vector2.zero;
    }
}

using System.Collections;
using System.Collections.Generic;
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

    }

    public override void UpdateService()
    {
        
    }

    public override void FixedUpdateService()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class TutorialSystem : SystemObjectWithVars<TutorialVars>
{
    public bool IsTutorialActive => isTutorialActive;
    private bool isTutorialActive;

    private float tutorialTimer = 0.0f;

    private FrogController tutorialFrog;

    public override void AwakeService()
    {
        
    }

    public override void StartService()
    {
        //isTutorialActive = true;
        SpawnFrog();
    }

    public override void UpdateService()
    {
        tutorialTimer += Time.deltaTime;

        if (tutorialFrog.GetState() == FrogController.State.InPond)
        {

        }
    }

    public override void FixedUpdateService()
    {
        
    }

    void SpawnFrog()
    {
        Vector2 vSpawnPos = GetVars().FrogSpawnPosition.position;

        GameObject newFrog = Object.Instantiate(Service.Vars<FrogSystemVars>().FrogPrefab, new Vector2(vSpawnPos.x, 9.0f), Quaternion.identity);
        tutorialFrog = newFrog.GetComponent<FrogController>();
        tutorialFrog.SpawnPosition = vSpawnPos;
        tutorialFrog.OnJustSpawned();
    }
}

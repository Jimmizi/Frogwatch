using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Com.LuisPedroFonseca.ProCamera2D;
using HutongGames.PlayMaker.Actions;
using MyBox;
using UnityEngine;
using UnityEngine.Experimental.AI;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class TutorialSystem : SystemObjectWithVars<TutorialVars>
{
    public bool IsTutorialActive => isTutorialActive;
    private bool isTutorialActive;

    private float tutorialTimer = 0.0f;
    private float showWitchAndFrogTimer = 0.0f;

    private FrogController tutorialFrog;
    private EnemyController tutorialWitch;

    private bool hiddenMoveText;
    private bool hiddenChargeText;

    public override void AwakeService()
    {
        
    }

    public override void StartService()
    {
        StartTutorial();

        GetVars().MoveText.alpha = 1.0f;
        GetVars().ChargeText.alpha = 0.0f;
        GetVars().ThrowText.alpha = 0.0f;
    }

    public override void UpdateService()
    {
        if (!isTutorialActive)
        { 
            return;
        }

        tutorialTimer += Time.deltaTime;


        if (!hiddenMoveText)
        {
            if (!GetVars().MoveTextBounds.OverlapPoint(HumanoidController.Player.GetOffsetPosition()))
            {
                hiddenMoveText = true;
                GetVars().StageOneText();
            }
        }
        else if (!hiddenChargeText)
        {
            if (tutorialWitch.GetState() == EnemyController.State.Stunned)
            {
                hiddenChargeText = true;
                GetVars().StageTwoText();
            }
        }


        bool bDone = false;
        
        if (tutorialFrog.GetState() == FrogController.State.InPond)
        {
            bDone = true;
        }
        else if (IsFrogOrPlayerOutOfBounds())
        {
            bDone = true;
        }
        
        if (bDone)
        {
            tutorialWitch.TriggerStunInstantEnd = true;
            EndTutorial();
        }
    }

    public override void FixedUpdateService()
    {
        
    }

    bool IsFrogOrPlayerOutOfBounds()
    {
        var frogbounds = GetVars().TutorialFrogBounds;
        var playerbounds = GetVars().TutorialPlayerBounds;

        if (!playerbounds.OverlapPoint(HumanoidController.Player.GetOffsetPosition()))
        {
            return true;
        }

        if (tutorialFrog.GetState() == FrogController.State.Idle 
            && !frogbounds.OverlapPoint(tutorialFrog.GetOffsetPosition()))
        {
            return true;
        }

        return false;
    }

    void StartTutorial()
    {
        isTutorialActive = true;

        GetVars().TutorialFrogBounds.gameObject.SetActive(true);
        foreach (var b in GetVars().TutorialBoundaries)
        {
            b.SetActive(true);
        }

        // Just make sure everything is as it should be
        var proCam = Camera.main.GetComponent<ProCamera2D>();
        Debug.Assert(proCam != null);
        Debug.Assert(proCam.CameraTargets.Count == 2);
        Debug.Assert(proCam.CameraTargets[0].TargetInfluenceH == 0.0f);
        Debug.Assert(Math.Abs(proCam.CameraTargets[1].TargetInfluenceH - 1.0f) < 0.01f);

        proCam.CameraTargets[0].TargetInfluenceH = 0.0f;
        proCam.CameraTargets[0].TargetInfluenceV = 0.0f;
        proCam.CameraTargets[1].TargetInfluenceH = 1.0f;
        proCam.CameraTargets[1].TargetInfluenceV = 1.0f;

        SpawnFrog();
        SpawnWitch();
    }

    void EndTutorial()
    {
        isTutorialActive = false;

        GetVars().TutorialFrogBounds.gameObject.SetActive(false);
        foreach (var b in GetVars().TutorialBoundaries)
        {
            b.SetActive(false);
        }

        var proCam = Camera.main.GetComponent<ProCamera2D>();

        // Switch focus to player target
        proCam.CameraTargets[0].TargetInfluenceH = 1.0f;
        proCam.CameraTargets[0].TargetInfluenceV = 1.0f;
        proCam.CameraTargets[1].TargetInfluenceH = 0.0f;
        proCam.CameraTargets[1].TargetInfluenceV = 0.0f;

        GetVars().ToDisableWhenDone.SetActive(false);
    }

    void SpawnFrog()
    {
        Vector2 vSpawnPos = GetVars().FrogSpawnPosition.position;

        GameObject newFrog = Object.Instantiate(Service.Vars<FrogSystemVars>().FrogPrefab, new Vector2(vSpawnPos.x, 9.0f), Quaternion.identity);
        tutorialFrog = newFrog.GetComponent<FrogController>();
        tutorialFrog.SpawnPosition = vSpawnPos;
        tutorialFrog.InstantSpawn = true;
        tutorialFrog.OnJustSpawned();

        tutorialFrog.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }

    void SpawnWitch()
    {
        Vector2 vSpawnPos = GetVars().FrogSpawnPosition.position;

        GameObject witch = Object.Instantiate(Service.Vars<FrogSystemVars>().WitchPrefab, vSpawnPos, Quaternion.identity);
        tutorialWitch = witch.GetComponent<EnemyController>();
        tutorialWitch.ExternalSetPosition(vSpawnPos);
        tutorialWitch.OnJustSpawned();

        tutorialWitch.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        if (tutorialWitch.SmokeDisappearAnimator != null)
        {
            Service.Get<AudioSystem>().PlayEvent(AudioEvent.SmokePuff, tutorialWitch.transform.position);
            tutorialWitch.SmokeDisappearAnimator.SetTrigger("Start");
        }
        
        GetVars().StartWitchFrogFadeIn(tutorialWitch.GetComponent<SpriteRenderer>(), tutorialFrog.GetComponent<SpriteRenderer>());

        tutorialWitch.TutorialFlee(tutorialFrog);
    }

    


    
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : HumanoidController
{
    private Vector2 vCurrentDirection;
    private Vector2 vCurrentTarget;



    public enum State
    {
        Idle,       // Taking a lil break
        Wandering,  // Wandering around the field
        Chasing,    // Has found a frog, chasing it
        Fleeing,    // Has a frog, trying to flee
        Stunned     // Has been stunned
    }
    public State GetState()
    {
        return state;
    }
    private State state;

    protected override void Start()
    {
        state = State.Idle;

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.Wandering:
                break;
            case State.Chasing:
                break;
            case State.Fleeing:
                break;
            case State.Stunned:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //float fHorizontal = Input.GetAxis("Horizontal");
        //float fVertical = Input.GetAxis("Vertical");
        //bool bInteracted = Input.GetButtonDown("Interact");

        InputDirection = vCurrentDirection;
        //JustPressedInteract = bInteracted;

        base.Update();
    }

    void ProcessIdle()
    {

    }

    void ProcessWandering()
    {

    }

    void ProcessChasing()
    {

    }

    void ProcessFleeing()
    {

    }

    void ProcessStunned()
    {

    }

    
}

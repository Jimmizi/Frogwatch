using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

public class FrogController : HumanoidController
{
    public enum State
    {
        Idle,
        Hopping,
        Carried,
        Thrown,
        InPond
    }
    
    private FrogSystemVars tuning;

    private float nextHopInterval;
    private float hopTimer;
    private float fThrowTime;
    private bool bFirstThrowSectionDone;

    public State GetState()
    {
        return state;
    }
    private State state;


    //public Vector2 DirectionTest = new();
    //public float DotToPlayer = 0.0f;

    // Start is called before the first frame update
    protected override void Start()
    {
        tuning = Service.Vars<FrogSystemVars>();
        ResetTimer();
        state = State.Idle;

        base.Start();
    }

    void ResetTimer()
    {
        nextHopInterval = GetNextHopInterval();
        hopTimer = 0.0f;
    }

    public void SetCarried()
    {
        state = State.Carried;
        m_animator.SetBool("IsCarried", true);
    }

    public void SetDropped()
    {
        
    }

    public void SetThrown(Vector2 dir)
    {
        StartCoroutine(PerformThrown(dir));
    }

    public bool CanPickup()
    {
        return state == State.Idle || state == State.Hopping;
    }

    public bool ShouldDrawInFrontDuringThrow()
    {
        return bFirstThrowSectionDone && fThrowTime < 0.45f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        switch (state)
        {
            case State.Idle:
            {
                TryContainFrogs();

                hopTimer += Time.deltaTime;
                if (hopTimer >= nextHopInterval)
                {
                    TryPerformHop();
                    ResetTimer();
                }

                break;
            }
        case State.Hopping:
                break;
            case State.Carried:
                break;
            case State.Thrown:
            {
                break;
            }
            case State.InPond:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        base.Update();
    }

    private void TryContainFrogs()
    {
        BoxCollider2D frogBounds = tuning.FrogMovementBounds;
        
        if (!frogBounds.OverlapPoint(m_rigidbody.position))
        {
            Vector2 vClosestPoint = frogBounds.ClosestPoint(m_rigidbody.position);
            Vector2 vDirToCenter = frogBounds.offset - vClosestPoint;
            vDirToCenter.Normalize();

            vClosestPoint += vDirToCenter * 2;
            m_rigidbody.position = vClosestPoint;
        }
    }

    protected override void FixedUpdate()
    {
    }

    void TryPerformHop()
    {
        Vector2 dir = GetHopDirection();
        Vector2 vPos = transform.position;

        StartCoroutine(PerformHop(vPos + (dir * tuning.HopDistance)));
    }

    IEnumerator PerformThrown(Vector2 vDir)
    {
        state = State.Thrown;

        Vector2 heightOffset = new Vector2(0.0f, 1.0f);
        Vector2 vOriginalPos = m_rigidbody.position;
        Vector2 vNewPos = Player.GetOffsetPosition() + (vDir * tuning.ThrownDistance) + heightOffset;

        BoxCollider2D frogBounds = tuning.FrogMovementBounds;

        bool newIsInvalid = false;
        bool originalIsInvalid = false;
        
        if (!frogBounds.OverlapPoint(vNewPos))
        {
            newIsInvalid = true;
        }
        if (!frogBounds.OverlapPoint(vNewPos))
        {
            originalIsInvalid = true;
        }
        if (newIsInvalid)
        {
            vNewPos = (originalIsInvalid ? GetPlayerPosition() : vOriginalPos);
        }

        fThrowTime = 0.0f;
        bFirstThrowSectionDone = false;

        while (fThrowTime <= 1.0f)
        {
            m_rigidbody.position = Easer.EaseVector2(tuning.ThrownEaser, vOriginalPos, vNewPos, fThrowTime);

            fThrowTime += Time.deltaTime * tuning.ThrownSpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        fThrowTime = 0.0f;
        bFirstThrowSectionDone = true;

        while (fThrowTime <= 1.0f)
        {
            m_rigidbody.position = Easer.EaseVector2(tuning.ThrownEaserSecondary, vNewPos, vNewPos - heightOffset, fThrowTime);

            fThrowTime += Time.deltaTime * (tuning.ThrownSpeed * 2);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        m_rigidbody.position = vNewPos - heightOffset;
        state = State.Idle;
        ResetTimer();

        // TODO Add into pond where if new pos is situated in a pond

        m_animator.SetBool("IsCarried", false);
        m_animator.SetBool("IsHopping", false);
    }

    IEnumerator PerformHop(Vector2 vNewPos)
    {
        state = State.Hopping;
        m_animator.SetBool("IsHopping", true);

        Vector2 vOriginalPos = m_rigidbody.position;
        
        float fTime = 0.0f;
        bool bAbort = false;

        while (fTime <= 1.0f)
        {
            m_rigidbody.position = Easer.EaseVector2(tuning.MovementEaser, vOriginalPos, vNewPos, fTime);
            
            fTime += Time.deltaTime * tuning.HopMovementSpeed;

            if (state != State.Hopping)
            {
                bAbort = true;
                break;
            }
            
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (!bAbort)
        {
            m_rigidbody.position = vNewPos;
            state = State.Idle;
        }

        m_animator.SetBool("IsHopping", false);
    }

    private float GetNextHopInterval()
    {
        return Random.Range(tuning.MinTimeBetweenHops, tuning.MaxTimeBetweenHops);
    }
    
    private Vector2 GetHopDirection()
    {
        int NumDirectionsToScore = 10;

        Dictionary<Vector2, float> directions = new();
        List<HumanoidController> nearbyEnts = HumanoidController.GetControllersInArea(transform.position, tuning.NearbyAvoidRadius);

        BoxCollider2D frogBounds = tuning.FrogMovementBounds;

        for (int i = 0; i < NumDirectionsToScore; ++i)
        {
            Vector2 vRandomDir = Random.insideUnitCircle.normalized;
            Vector2 vThisPos = transform.position;

            if (directions.ContainsKey(vRandomDir))
            {
                continue;
            }

            Vector2 vPossibleNewPosition = vThisPos + (vRandomDir * tuning.HopDistance);
            if (!frogBounds.OverlapPoint(vPossibleNewPosition))
            {
                continue;
            }

            float fScore = 0.0f;
            float fFacingDot = 0.0f;
            float fNonFacingDot = 0.0f;

            // Higher the score the worst
            for (int y = 0; y < nearbyEnts.Count; ++y)
            {
                Vector2 vEntPos = nearbyEnts[y].transform.position;

                Vector2 vDirToEnt = vEntPos - vThisPos;
                vDirToEnt.Normalize();
                
                float fDot = Vector2.Dot(vRandomDir, vDirToEnt);
                
                if (fDot > 0.0f)
                {
                    fFacingDot += fDot;
                }
                else
                {
                    fNonFacingDot += -fDot;
                }
            }

            if (fFacingDot > fNonFacingDot)
            {
                fScore += fFacingDot;
            }
            else if (fFacingDot < fNonFacingDot)
            {
                fScore -= fNonFacingDot;
            }

            directions.Add(vRandomDir, fScore);
        }

        float fBestScore = 999.0f;
        Vector2 vBestDir = Vector2.zero;

        foreach (var score in directions)
        {
            if (score.Value < fBestScore)
            {
                fBestScore = score.Value;
                vBestDir = score.Key;
            }
        }

        return vBestDir;
    }

    protected override void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0.9215686f, 0.01568628f, 0.25f);
        Gizmos.DrawSphere(transform.position, tuning.NearbyAvoidRadius);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        //List<HumanoidController> nearbyEnts = HumanoidController.GetControllersInArea(transform.position, NearbyAvoidRadius);
        //if (nearbyEnts.Count == 1)
        //{
        //    Vector2 vEntPos = nearbyEnts[0].transform.position;
        //    Vector2 vThisPos = transform.position;

        //    Vector2 vDirToEnt = vEntPos - vThisPos;
        //    vDirToEnt.Normalize();

        //    float fDot = Vector2.Dot(DirectionTest, vDirToEnt);
        //    DotToPlayer = fDot;

        //    Gizmos.color = Color.cyan;
        //    Gizmos.DrawLine(transform.position, transform.position + new Vector3(vDirToEnt.x, vDirToEnt.y, 0.0f));
        //}

        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, transform.position + new Vector3(DirectionTest.x, DirectionTest.y, 0.0f));
    }

    void OnGUI()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        //Vector2 vScreenPos = Camera.main.WorldToScreenPoint(transform.position);

        //GUI.Label(new Rect(vScreenPos, new Vector2(100,100)), $"{hopTimer}/{nextHopInterval}");
    }
    
}

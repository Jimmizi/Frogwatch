using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public static List<FrogController> FrogList = new();

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
        FrogList.Add(this);

        ResetTimer();
        state = State.Idle;

        base.Start();
    }

    protected override void OnDestroy()
    {
        FrogList.Remove(this);
        base.OnDestroy();
    }

    void ResetTimer()
    {
        nextHopInterval = GetNextHopInterval();
        hopTimer = 0.0f;
    }

    public void SetCarried(bool bHeldByWitch = false)
    {
        state = State.Carried;
        m_animator.SetBool("IsCarried", true);
        if (bHeldByWitch)
        {
            m_animator.SetBool("HeldByWitch", true);
        }
    }

    public void SetDropped()
    {
        m_animator.SetBool("IsCarried", false);
        m_animator.SetBool("HeldByWitch", false);
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
    protected new void Update()
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
    }

    private void TryContainFrogs()
    {
        BoxCollider2D frogBounds = GetVars().FrogMovementBounds;
        
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

        StartCoroutine(PerformHop(vPos + (dir * GetVars().HopDistance)));
    }

    IEnumerator PerformThrown(Vector2 vDir)
    {
        state = State.Thrown;

        Vector2 heightOffset = new Vector2(0.0f, 0.25f);
        Vector2 vOriginalPos = m_rigidbody.position;
        Vector2 vNewPos = Player.GetOffsetPosition() + (vDir * GetVars().ThrownDistance) + heightOffset;

        BoxCollider2D frogBounds = GetVars().FrogMovementBounds;

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
            m_rigidbody.position = Easer.EaseVector2(GetVars().ThrownEaser, vOriginalPos, vNewPos, fThrowTime);

            fThrowTime += Time.deltaTime * GetVars().ThrownSpeed;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        fThrowTime = 0.0f;
        bFirstThrowSectionDone = true;

        while (fThrowTime <= 1.0f)
        {
            m_rigidbody.position = Easer.EaseVector2(GetVars().ThrownEaserSecondary, vNewPos, vNewPos - heightOffset, fThrowTime);

            fThrowTime += Time.deltaTime * (GetVars().ThrownSpeed * 2);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        m_rigidbody.position = vNewPos - heightOffset;
        state = State.Idle;
        ResetTimer();

        // TODO Add into pond where if new pos is situated in a pond

        m_animator.SetBool("IsCarried", false);
        m_animator.SetBool("IsHopping", false);
        m_animator.SetBool("HeldByWitch", false);
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
            m_rigidbody.position = Easer.EaseVector2(GetVars().MovementEaser, vOriginalPos, vNewPos, fTime);
            
            fTime += Time.deltaTime * GetVars().HopMovementSpeed;

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
        return Random.Range(GetVars().MinTimeBetweenHops, GetVars().MaxTimeBetweenHops);
    }
    
    private Vector2 GetHopDirection()
    {
        int NumDirectionsToScore = 10;

        Dictionary<Vector2, float> directions = new();
        List<HumanoidController> nearbyEnts = HumanoidController.GetControllersInArea(transform.position, GetVars().NearbyAvoidRadius);

        BoxCollider2D frogBounds = GetVars().FrogMovementBounds;
        Vector2 vThisPos = GetOffsetPosition();
        
        for (int i = 0; i < NumDirectionsToScore; ++i)
        {
            Vector2 vRandomDir = Random.insideUnitCircle.normalized;
            if (directions.ContainsKey(vRandomDir))
            {
                continue;
            }

            Vector2 vPossibleNewPosition = vThisPos + (vRandomDir * GetVars().HopDistance);
            if (!frogBounds.OverlapPoint(vPossibleNewPosition))
            {
                continue;
            }

            float fScore = 0.0f;
            float fFacingDot = 0.0f;
            float fHorizontalRatio = (frogBounds.offset.x + vThisPos.x) / (frogBounds.offset.x + (frogBounds.size.x / 2));
            float fVerticalRatio = (frogBounds.offset.y + vThisPos.y) / (frogBounds.offset.y + (frogBounds.size.y / 2));
            int iEdgeHits = 0;

            bool extremeEdge = (fHorizontalRatio > 0.75f || fVerticalRatio > 0.75f);

            // Higher the score the worse
            for (int y = 0; y < nearbyEnts.Count; ++y)
            {
                bool isFrog = nearbyEnts[y].ZSort.IsFrog;

                Vector2 vEntPos = nearbyEnts[y].transform.position;

                Vector2 vDirToEnt = vEntPos - vThisPos;
                vDirToEnt.Normalize();

                float fDot = Vector2.Dot(vRandomDir, vDirToEnt);
                fFacingDot += isFrog ? fDot * 0.2f : -fDot * (extremeEdge ? 0.1f : 0.5f);
            }

            if (nearbyEnts.Count > 0)
            {
                fFacingDot /= nearbyEnts.Count;
            }

            fScore += fFacingDot;
            
            // If more than 75% to the horizontal edge
            if (fHorizontalRatio > 0.65f)
            {
                // If on the right and going right
                if (vThisPos.x > 0 && vRandomDir.x > 0.0f)
                {
                    ++iEdgeHits;


                    if (fHorizontalRatio > 0.85f)
                    {
                        fScore = 0.0f;
                    }
                }
                // If on the left and going left
                else if (vThisPos.x < 0 && vRandomDir.x < 0.0f)
                {
                    ++iEdgeHits;


                    if (fHorizontalRatio > 0.85f)
                    {
                        fScore = 0.0f;
                    }
                }
                else
                {
                    fScore += 0.3f;
                }

            }

            // If more than 75% to the vertical edge
            if (fVerticalRatio > 0.65f)
            {
                // If above and going above
                if (vThisPos.y > 0 && vRandomDir.y > 0.0f)
                {
                    ++iEdgeHits;

                    if (fVerticalRatio > 0.85f)
                    {
                        fScore = 0.0f;
                    }
                }
                // If below and going below
                else if (vThisPos.y < 0 && vRandomDir.y < 0.0f)
                {
                    ++iEdgeHits;

                    if (fVerticalRatio > 0.85f)
                    {
                        fScore = 0.0f;
                    }
                }
                else
                {
                    fScore += 0.3f;
                }
            }
            
            fScore /= 1 + iEdgeHits;

            // Try keep them towards the middle
            //float fDotTowardsMiddle = Vector2.Dot(vRandomDir, frogBounds.offset);
            //fScore -= fDotTowardsMiddle * Random.Range(0.5f, 2.0f);

            directions.Add(vRandomDir, fScore);
        }

        float fBestScore = 0.0f;
        Vector2 vBestDir = Vector2.zero;

        foreach (var score in directions)
        {
            if (score.Value > fBestScore)
            {
                fBestScore = score.Value;
                vBestDir = score.Key;
            }
        }
        
        List<Vector2> vDirectionsToTry = new();
        for (int i = 0; i < directions.Count; ++i)
        {
            vDirectionsToTry.Add(directions.Keys.ElementAt(i));
            vDirectionsToTry.Add(vBestDir);
        }

        return vDirectionsToTry.Count == 0 ? Vector2.zero : vDirectionsToTry[Random.Range(0, vDirectionsToTry.Count)];
    }

    protected override void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0.1215686f, 0.01568628f, 0.25f);
        Gizmos.DrawSphere(transform.position, GetVars().NearbyAvoidRadius);
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

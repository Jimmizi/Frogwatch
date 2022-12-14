
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : HumanoidController
{
    private Vector2 vCurrentDirection;

    public static List<EnemyController> Witches = new();

    [HideInInspector]
    public FrogController targetFrog;

    private Vector2 vCourseCorrectedPosition;
    private float fCourseCorrectingUpdateTick;
    private float fTimeSpentCourseCorrecting;
    private int numRecentCourseCorrectionSpams;

    public Animator SmokeDisappearAnimator;

    private Vector2 vFleeTarget;

    private RaycastHit2D debugLastHit;
    private Vector2 debugLastHitOrigin;

    public enum State
    {
        Idle,       // Taking a lil break
        Wandering,  // Wandering around the field
        Chasing,    // Has found a frog, chasing it
        Fleeing,    // Has a frog, trying to flee
        Stunned,    // Has been stunned

        SuccessfulFlee,

        Finished
    }
    private State state;

    public State GetState()
    {
        return state;
    }


    private float currentStateTimer = 0.0f;
    private float timeToStayInState = 0.0f;
    private float wanderFrogCheckTimer = 0.0f;
    private float wanderInDirectionTimer;

    private float timeSinceLastCourseCorrection = 0.0f;

    private float currentSpeedMod = 1.0f;
    private float speedModTimer;

    void SetState(State eNew)
    {
        state = eNew;
        currentStateTimer = 0.0f;

        fTimeSpentCourseCorrecting = 0.0f;
        fCourseCorrectingUpdateTick = 0.0f;
        vCourseCorrectedPosition = Vector2.zero;
    }
    float GetTimeInState()
    {
        return currentStateTimer;
    }

    float GetPathfindingAheadDistance()
    {
        float pathfindingLookAheadDist = 0.75f;

        if (state == State.Chasing)
        {
            pathfindingLookAheadDist = 0.2f;
        }

        return pathfindingLookAheadDist;
    }

    protected override void Start()
    {
        Witches.Add(this);

        SetState(State.Idle);
        timeToStayInState = GetNextTimeToIdle();

        base.Start();
    }

    protected override void OnDestroy()
    {
        Witches.Remove(this);

        base.OnDestroy();
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (StatsScreen.IsOnStatsScreen)
        {
            return;
        }
        else if (Service.Get<TutorialSystem>().IsTutorialActive)
        {
            return;
        }

        currentStateTimer += Time.deltaTime;
        speedModTimer -= Time.deltaTime;

        timeSinceLastCourseCorrection += Time.deltaTime;

        if (speedModTimer <= 0.0f)
        {
            speedModTimer = Random.Range(1.0f, 2.0f);
            currentSpeedMod = Random.Range(0.8f, 1.2f);
        }

        switch (state)
        {
            case State.Idle:
                ProcessIdle();
                break;
            case State.Wandering:
                ProcessWandering();
                break;
            case State.Chasing:
                ProcessChasing();
                break;
            case State.Fleeing:
                ProcessFleeing();
                break;
            case State.Stunned:
                ProcessStunned();
                break;
            case State.SuccessfulFlee:
                ProcessSuccessfulFlee();
                break;
            case State.Finished:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        InputDirection = vCurrentDirection;
        //JustPressedInteract = bInteracted;

        base.Update();
    }

    protected override float GetCurrentSpeedMult()
    {
        switch (state)
        {
            case State.Wandering:
                return GetVars().WitchWanderSpeed * currentSpeedMod;
            case State.Chasing:
                return GetVars().WitchChaseSpeed * currentSpeedMod;
            case State.Fleeing:
                return GetVars().WitchFleeSpeed * currentSpeedMod;

            case State.Idle:
            case State.Stunned:
            case State.SuccessfulFlee:
            case State.Finished:
                return 0.0f;
        }

        return base.GetCurrentSpeedMult() * currentSpeedMod;
    }

    protected override bool CanLeaveBounds()
    {
        return state == State.Fleeing || state == State.SuccessfulFlee;
    }

    public void SetDashedInto()
    {
        if (state != State.Stunned && state != State.SuccessfulFlee && state != State.Finished)
        {
            GameStats.RecordWitchBonk();
            Service.Get<AudioSystem>().PlayEvent(AudioEvent.DashIntoWitch, transform.position);

            DropCarriedFrog(true);
            targetFrog = null;

            SetState(State.Stunned);
            timeToStayInState = GetNextTimeToStun();
            vCurrentDirection = Vector2.zero;
            vCourseCorrectedPosition = Vector2.zero;
            

            SetAnimCarrying(false);
            SetAnimWalking(false);

            m_animator.SetBool("IsStunned", true);
        }
    }

    void InitWander()
    {
        vCurrentDirection = GetNewWanderDirection();
        timeToStayInState = GetNextTimeToWander();
        wanderInDirectionTimer = -1.0f;
    }

    void InitIdle()
    {
        timeToStayInState = GetNextTimeToIdle();
        vCurrentDirection = Vector2.zero;
    }

    public void TutorialFlee(FrogController frog)
    {
        SetCarryingFrog(frog, true);
        SetState(State.Fleeing);
        var vRandoCorner = GetRandomBoundsEdge();
        var vDir = vRandoCorner - GetOffsetPosition();
        vDir.Normalize();

        vFleeTarget = vRandoCorner + (vDir * 0.2f);
        vFleeTarget = vRandoCorner;
        vCurrentDirection = Vector2.zero;
    }

    void InitFlee()
    {
        SetCarryingFrog(targetFrog, true);
        targetFrog = null;

        Service.Get<AudioSystem>().PlayEvent(AudioEvent.WitchPickedFrog, transform.position);

        SetState(State.Fleeing);

        var vRandoCorner = GetRandomBoundsEdge();
        var vDir = vRandoCorner - GetOffsetPosition();
        vDir.Normalize();

        vFleeTarget = vRandoCorner + (vDir * 0.2f);
        vFleeTarget = vRandoCorner;
        vCurrentDirection = Vector2.zero;
    }

    void ProcessIdle()
    {
        if (GetTimeInState() > timeToStayInState)
        {
            SetState(State.Wandering);
            InitWander();
        }
    }

    void ProcessWandering()
    {
        wanderFrogCheckTimer += Time.deltaTime;
        wanderInDirectionTimer -= Time.deltaTime;

        if (GetTimeInState() > timeToStayInState)
        {
            SetState(State.Idle);
            InitIdle();
        }

        if (wanderFrogCheckTimer >= 1.0f)
        {
            wanderFrogCheckTimer = 0.0f;
            if (Random.Range(0.0f, 100.0f) < 75.0f)
            {
                var tempFrog = GetBestFrogToHunt();

                if (tempFrog != null)
                {
                    targetFrog = tempFrog;
                    SetState(State.Chasing);
                }
            }
        }

        if (!IsCourseCorrecting())
        {
            // Alter the direction every now and then
            if (wanderInDirectionTimer <= 0.0f)
            {
                wanderInDirectionTimer = 1.0f;

                Vector2 vMod = new Vector2(Random.Range(-2.0f, 2.0f), Random.Range(-2.0f, 2.0f));
                vCurrentDirection += new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f)) * vMod;

                vCurrentDirection.Normalize();
            }

            ProcessCourseCorrection();
        }
        else
        {
            WaitForCourseCorrection();
        }
    }

    void ProcessChasing()
    {
        // If frog is invalid now, or too long has been spent trying to catch the frog, stop
        if (!targetFrog.CanPickup() || GetTimeInState() > 15.0f)
        {
            SetState(State.Idle);
            InitIdle();
            targetFrog = null;
            return;
        }

        Vector2 vPos = GetOffsetPosition();
        
        if (!IsCourseCorrecting())
        {
            var frogPos = targetFrog.GetOffsetPosition();
        
            var desiredDir = frogPos - GetOffsetPosition();
            desiredDir.Normalize();

            vCurrentDirection = desiredDir;

            if (Vector2.Distance(vPos, frogPos) <= PickupDistance)
            {
                InitFlee();
            }

            ProcessCourseCorrection();
        }
        else
        {
            WaitForCourseCorrection();
        }
    }

    void ProcessFleeing()
    {
        // First second idle
        if (GetTimeInState() <= 1.0f)
        {
            return;
        }
        
        if (!IsCourseCorrecting())
        {
            Vector2 vPos = GetOffsetPosition();
            var desiredDir = vFleeTarget - GetOffsetPosition();
            desiredDir.Normalize();
            
            vCurrentDirection = desiredDir;

            ProcessCourseCorrection();

            if (Vector2.Distance(vPos, vFleeTarget) < PickupDistance)
            {
                SetState(State.SuccessfulFlee);
                vCurrentDirection = Vector2.zero;
            }
        }
        else
        {
            WaitForCourseCorrection();
        }
    }

    [HideInInspector]
    public bool TriggerStunInstantEnd = false;

    void ProcessStunned()
    {
        if (GetTimeInState() > timeToStayInState || TriggerStunInstantEnd)
        {
            TriggerStunInstantEnd = false;
            m_animator.SetBool("IsStunned", false);
            SetState(State.Idle);
            InitIdle();
        }
    }

    void ProcessSuccessfulFlee()
    {
        ++FrogController.NumFrogsTaken;

        SetState(State.Finished);
        vCurrentDirection = Vector2.zero;

        StartCoroutine(DoFleeEnding());
    }

    IEnumerator DoFleeEnding()
    {
        float fTime = 0.0f;

        List<SpriteRenderer> frogRenderers = new();

        var frogSr = FrogCarrying.GetComponent<SpriteRenderer>();
        if (frogSr != null)
        {
            frogRenderers.Add(frogSr);
        }
        
        SpriteRenderer[] frogRenders = FrogCarrying.GetComponentsInChildren<SpriteRenderer>();
        if (frogRenders.Length > 0)
        {
            frogRenderers.AddRange(frogRenders);
        }

        SpriteRenderer witchRender = GetComponent<SpriteRenderer>();

        void MinusAlpha(SpriteRenderer rend, float a)
        {
            Color col = rend.color;

            if (col.a > 0.0f)
            {
                col.a -= a;
                rend.color = col;
            }
        }

        if (SmokeDisappearAnimator != null)
        {
            Service.Get<AudioSystem>().PlayEvent(AudioEvent.SmokePuff, transform.position);
            SmokeDisappearAnimator.SetTrigger("Start");
        }

        while (fTime < 1.0f)
        {
            fTime += Time.deltaTime;

            if (fTime >= 0.25f)
            {
                foreach (var fr in frogRenderers)
                {
                    if (fr != null)
                    {
                        MinusAlpha(fr, (Time.deltaTime) * 5);
                    }
                }

                if (witchRender != null)
                {
                    MinusAlpha(witchRender, (Time.deltaTime) * 5);
                }
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
        
        Destroy(FrogCarrying.gameObject); // :(
        Destroy(gameObject); // :)
    }

    float GetNextTimeToIdle()
    {
        var vars = GetVars();
        return Random.Range(vars.WitchIdleMinTime, vars.WitchIdleMaxTime);
    }

    float GetNextTimeToStun()
    {
        var vars = GetVars();
        return Random.Range(vars.WitchStunnedMinTime, vars.WitchStunnedMaxTime);
    }
    float GetNextTimeToWander()
    {
        var vars = GetVars();
        return Random.Range(vars.WitchWanderMinTime, vars.WitchWanderMaxTime);
    }

    Vector2 GetNewWanderDirection()
    {
        BoxCollider2D bounds = GetVars().FrogMovementBounds;
        var vThisPos = GetOffsetPosition();
        Vector2 vAvgFrogPos = Service.Get<FrogManagerSystem>().averageFrogPosition;
        Vector2 vDirToFrogs = vAvgFrogPos - vThisPos;
        vDirToFrogs.Normalize();

        float fAheadDist = GetPathfindingAheadDistance();

        Dictionary<Vector2, float> directions = new();
        for (int i = 0; i < 10; ++i)
        {
            Vector2 vRandomDir = Random.insideUnitCircle.normalized;
            Vector2 vFuturePos = vThisPos + (vRandomDir * fAheadDist);

            if (directions.ContainsKey(vRandomDir))
            {
                continue;
            }

            if (!bounds.OverlapPoint(vFuturePos))
            {
                continue;
            }

            if (DoesDirectionIntersectMapObject(vRandomDir))
            {
                continue;
            }

            float fScore = 0.0f;
            float fDot = Vector2.Dot(vRandomDir, vDirToFrogs);
            fScore += fDot;
            
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

        // Add the count of the best direction in order to weight it towards the best direction
        //  but won't necessarily always pick it
        List<Vector2> vDirectionsToPickFrom = new();

        for (int i = 0; i < directions.Count; ++i)
        {
            vDirectionsToPickFrom.Add(directions.Keys.ElementAt(i));
        }

        for (int i = 0; i < directions.Count; ++i)
        {
            vDirectionsToPickFrom.Add(vBestDir);
        }

        if (vDirectionsToPickFrom.Count > 0)
        {
            return vDirectionsToPickFrom[Random.Range(0, vDirectionsToPickFrom.Count)];
        }

        return Random.insideUnitCircle.normalized;
    }

    Vector2 GetRandomBoundsEdge()
    {
        BoxCollider2D coll = GetVars().FrogMovementBounds;
        
        List<Vector2> cornerPoints = new();

        Vector2 vWorldPos = new Vector2(coll.transform.position.x, coll.transform.position.y) + coll.offset;
        float halfHori = coll.size.x / 2;
        float halfVert = coll.size.y / 2;

        // Comment out to make witches not flee towards the top because of the player house (remove if player house not added)
        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori, halfVert));         // Top Left
        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori * 0.5f, halfVert));  // Top Middle Left
        //cornerPoints.Add(vWorldPos + new Vector2(0.0f, halfVert));              // Top Middle
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori * 0.5f, halfVert));   // Top Middle Right
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori, halfVert));          // Top Right
        
        cornerPoints.Add(vWorldPos + new Vector2(-halfHori, -halfVert));        // Bot Left
        cornerPoints.Add(vWorldPos + new Vector2(-halfHori * 0.5f, -halfVert)); // Bot Middle Left
        cornerPoints.Add(vWorldPos + new Vector2(0.5f, -halfVert));             // Bot Middle
        cornerPoints.Add(vWorldPos + new Vector2(halfHori * 0.5f, -halfVert));  // Bot Middle Right
        cornerPoints.Add(vWorldPos + new Vector2(halfHori, -halfVert));         // Bot Right

        cornerPoints.Add(vWorldPos + new Vector2(-halfHori, halfVert * 0.5f));  // Top Middle Left
        cornerPoints.Add(vWorldPos + new Vector2(-halfHori, 0.0f));             // Middle Left
        cornerPoints.Add(vWorldPos + new Vector2(-halfHori, -halfVert * 0.5f)); // Bottom Middle Left

        cornerPoints.Add(vWorldPos + new Vector2(halfHori, halfVert * 0.5f));   // Top Middle Right
        cornerPoints.Add(vWorldPos + new Vector2(halfHori, 0.0f));              // Middle Right
        cornerPoints.Add(vWorldPos + new Vector2(halfHori, -halfVert * 0.5f));  // Bottom Middle Right

        return cornerPoints[Random.Range(0, cornerPoints.Count)];

        //float minDist = Mathf.Infinity;
        //Vector2 nearestPoint = Vector2.zero;

        //foreach (Vector2 point in cornerPoints)
        //{
        //    float dist = Vector2.Distance(vPos, point);

        //    if (dist < minDist)
        //    {
        //        minDist = dist;
        //        nearestPoint = point;
        //    }
        //}

        //return nearestPoint;
    }
    Vector3 GetNearestColliderPoint(RaycastHit2D hit)
    {
        PolygonCollider2D pc = hit.collider as PolygonCollider2D;

        float minDistanceSqr = Mathf.Infinity;
        Vector3 nearestColliderPoint = Vector3.zero;

        // Scan all collider points to find nearest
        foreach (Vector3 colliderPoint in pc.points)
        {
            // Convert to world point
            Vector3 colliderPointWorld = hit.transform.TransformPoint(colliderPoint);

            Vector3 diff = hit.point - (Vector2)colliderPointWorld;
            float distSqr = diff.sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestColliderPoint = colliderPointWorld;
            }
        }

        return nearestColliderPoint;
    }

    bool IsCourseCorrecting()
    {
        return vCourseCorrectedPosition != Vector2.zero;
    }
    void WaitForCourseCorrection()
    {
        fCourseCorrectingUpdateTick += Time.deltaTime;
        fTimeSpentCourseCorrecting += Time.deltaTime;

        if (Vector2.Distance(GetOffsetPosition(), vCourseCorrectedPosition) < PickupDistance || fTimeSpentCourseCorrecting > 3.0f)
        {
            vCourseCorrectedPosition = Vector2.zero;
            fCourseCorrectingUpdateTick = 0.0f;
        }

        if (fCourseCorrectingUpdateTick >= 0.75f)
        {
            fCourseCorrectingUpdateTick = 0.0f;
            ProcessCourseCorrection(true);
        }
    }
    void ProcessCourseCorrection(bool bComingFromCourseCorrectionWait = false)
    {
        float fAheadDist = GetPathfindingAheadDistance();

        var vPos = GetOffsetPosition();
        var vFuturePos = vPos + (vCurrentDirection * fAheadDist);

        var overlappedObject = StaticObject.GetOverlappedIncrement(vFuturePos, -(vCurrentDirection * fAheadDist));
        if (overlappedObject != null)
        {
            if (timeSinceLastCourseCorrection < 0.5f)
            {
                ++numRecentCourseCorrectionSpams;
            }
            else
            {
                numRecentCourseCorrectionSpams = 0;
            }

            timeSinceLastCourseCorrection = 0.0f;

            bool bSpam = (numRecentCourseCorrectionSpams > 4);
            if (bSpam)
            {
                Debug.Log("Spamming ProcessCourseCorrection");
            }


            var filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("MapObject");
            filter.useLayerMask = true;

            RaycastHit2D[] results = new RaycastHit2D[25]; // does Raycast only go up to the count or will it crash if there are more hit?! damn unity blackbox

            Physics2D.Raycast(vPos - (vCurrentDirection * 0.05f), vCurrentDirection * fAheadDist, filter, results);
            foreach (var hit in results)
            {
                if (hit.collider != null)
                {
                    Vector2 vNearestCorner = GetNearestColliderPoint(hit);
                    Vector2 vDirFromHit = vNearestCorner - hit.point;
                    vDirFromHit.Normalize();

                    float fPushAmount = (bComingFromCourseCorrectionWait ? 0.1f : 0.2f);

                    if (bSpam)
                    {
                        fPushAmount += Random.Range(-0.05f, 0.5f);
                        vDirFromHit += Random.insideUnitCircle.normalized * 0.25f;
                    }

                    Vector2 vPushedPosition = vNearestCorner + vDirFromHit * fPushAmount;
                    
                    vCurrentDirection = vPushedPosition - vPos;
                    vCurrentDirection.Normalize();
                    vCourseCorrectedPosition = vPushedPosition;
                    fCourseCorrectingUpdateTick = 0.0f;

                    if (!bComingFromCourseCorrectionWait)
                    {
                        fTimeSpentCourseCorrecting = 0.0f;
                    }

                    debugLastHitOrigin = vPos;
                    debugLastHit = hit;

                    //// If the new position also overlaps, just pick a new direction
                    //var newOverlapped = StaticObject.GetOverlapped(vPos + (vCurrentDirection * fAheadDist));
                    //if (newOverlapped != null)
                    //{
                    //    vCurrentDirection = GetNewWanderDirection();
                    //}
                    
                    break;
                }
            }
        }
    }

    bool DoesDirectionIntersectMapObject(Vector2 dir)
    {
        float fAheadDist = GetPathfindingAheadDistance();
        var vPos = GetOffsetPosition();
        var vFuturePos = vPos + (vCurrentDirection * fAheadDist);

        var filter = new ContactFilter2D();
        filter.layerMask = LayerMask.GetMask("MapObject");
        filter.useLayerMask = true;

        RaycastHit2D[] results = new RaycastHit2D[25]; // does Raycast only go up to the count or will it crash if there are more hit?! damn unity blackbox

        Physics2D.Raycast(vPos, vCurrentDirection * fAheadDist, filter, results);
        foreach (var hit in results)
        {
            if (hit.collider != null)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnDrawGizmos()
    {
        //if (!Application.isPlaying)
        //{
        //    return;
        //}

        //if (state != State.Idle && state != State.Stunned)
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawLine(GetOffsetPosition() + new Vector2(0.0f, 0.05f), GetOffsetPosition() + (vCurrentDirection * GetPathfindingAheadDistance()) + new Vector2(0.0f, 0.05f));
        //}

        //if (debugLastHit.point != Vector2.zero)
        //{
        //    Vector2 vNearestCorner = GetNearestColliderPoint(debugLastHit);
        //    Vector2 vDirFromHit = vNearestCorner - debugLastHit.point;
        //    vDirFromHit.Normalize();

        //    Vector2 vPushedPosition = vNearestCorner + vDirFromHit * 0.15f;

        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(debugLastHitOrigin, debugLastHit.point);

        //    Gizmos.color = new Color(1.0f, 0.65f, 0.0f);
        //    Gizmos.DrawLine(debugLastHit.point, vNearestCorner);

        //    Gizmos.color = Color.yellow;
        //    Gizmos.DrawLine(vNearestCorner, vPushedPosition);
        //}

        //if (state == State.Fleeing)
        //{
        //    Gizmos.color = Color.white;
        //    Gizmos.DrawLine(GetOffsetPosition(), vFleeTarget);
        //}

        //BoxCollider2D coll = GetVars().FrogMovementBounds;

        //Vector2 vPos = GetOffsetPosition();
        //List<Vector2> cornerPoints = new();

        //Vector2 vWorldPos = new Vector2(coll.transform.position.x, coll.transform.position.y) + coll.offset;
        //float halfHori = coll.size.x / 2;
        //float halfVert = coll.size.y / 2;

        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori, halfVert));         // Top Left
        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori * 0.5f, halfVert));  // Top Middle Left
        //cornerPoints.Add(vWorldPos + new Vector2(0.0f, halfVert));              // Top Middle
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori * 0.5f, halfVert));   // Top Middle Right
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori, halfVert));          // Top Right

        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori, -halfVert));        // Bot Left
        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori * 0.5f, -halfVert)); // Bot Middle Left
        //cornerPoints.Add(vWorldPos + new Vector2(0.5f, -halfVert));             // Bot Middle
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori * 0.5f, -halfVert));  // Bot Middle Right
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori, -halfVert));         // Bot Right

        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori, halfVert * 0.5f));  // Top Middle Left
        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori, 0.0f));             // Middle Left
        //cornerPoints.Add(vWorldPos + new Vector2(-halfHori, -halfVert * 0.5f)); // Bottom Middle Left

        //cornerPoints.Add(vWorldPos + new Vector2(halfHori, halfVert * 0.5f));   // Top Middle Right
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori, 0.0f));              // Middle Right
        //cornerPoints.Add(vWorldPos + new Vector2(halfHori, -halfVert * 0.5f));  // Bottom Middle Right

        //foreach (var point in cornerPoints)
        //{
        //    Gizmos.DrawSphere(point, 0.1f);
        //}

        base.OnDrawGizmos();
    }
}

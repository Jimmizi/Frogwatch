
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
    public FrogController targetFrog;
    private Vector2 vCourseCorrectedPosition;

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

        SuccessfulFlee
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

    private float currentSpeedMod = 1.0f;
    private float speedModTimer;

    void SetState(State eNew)
    {
        state = eNew;
        currentStateTimer = 0.0f;
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
        SetState(State.Idle);
        timeToStayInState = GetNextTimeToIdle();

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        currentStateTimer += Time.deltaTime;
        speedModTimer -= Time.deltaTime;

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
                return 0.0f;
        }

        return base.GetCurrentSpeedMult() * currentSpeedMod;
    }

    protected override bool CanLeaveBounds()
    {
        return state == State.Fleeing;
    }

    public void SetDashedInto()
    {
        DropCarriedFrog();
        SetState(State.Stunned);
        timeToStayInState = GetNextTimeToStun();

        SetAnimCarrying(false);
        SetAnimWalking(false);
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

    void InitFlee()
    {
        SetCarryingFrog(targetFrog, true);
        targetFrog = null;

        SetState(State.Fleeing);

        var vNearestCorner = GetRandomBoundsEdge();
        var vDir = vNearestCorner - GetOffsetPosition();
        vDir.Normalize();

        vFleeTarget = vNearestCorner + (vDir * 2);
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

    void ProcessChasing()
    {
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

    void ProcessStunned()
    {
        if (GetTimeInState() > timeToStayInState)
        {
            SetState(State.Wandering);
            InitWander();
        }
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

        Vector2 vPos = GetOffsetPosition();
        List<Vector2> cornerPoints = new();

        Vector2 vWorldPos = new Vector2(coll.transform.position.x, coll.transform.position.y) + coll.offset;
        float halfHori = coll.size.x / 2;
        float halfVert = coll.size.y / 2;

        cornerPoints.Add(vWorldPos + new Vector2(-halfHori, halfVert));         // Top Left
        cornerPoints.Add(vWorldPos + new Vector2(-halfHori * 0.5f, halfVert));  // Top Middle Left
        cornerPoints.Add(vWorldPos + new Vector2(0.0f, halfVert));              // Top Middle
        cornerPoints.Add(vWorldPos + new Vector2(halfHori * 0.5f, halfVert));   // Top Middle Right
        cornerPoints.Add(vWorldPos + new Vector2(halfHori, halfVert));          // Top Right
        
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
        if (Vector2.Distance(GetOffsetPosition(), vCourseCorrectedPosition) < PickupDistance)
        {
            vCourseCorrectedPosition = Vector2.zero;
        }
    }
    void ProcessCourseCorrection()
    {
        float fAheadDist = GetPathfindingAheadDistance();

        var vPos = GetOffsetPosition();
        var vFuturePos = vPos + (vCurrentDirection * fAheadDist);

        var overlappedObject = StaticObject.GetOverlapped(vFuturePos);
        if (overlappedObject != null)
        {
            var filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("MapObject");
            filter.useLayerMask = true;

            RaycastHit2D[] results = new RaycastHit2D[25]; // does Raycast only go up to the count or will it crash if there are more hit?! damn unity blackbox

            Physics2D.Raycast(vPos, vCurrentDirection * fAheadDist, filter, results);
            foreach (var hit in results)
            {
                if (hit.collider != null)
                {
                    Vector2 vNearestCorner = GetNearestColliderPoint(hit);
                    Vector2 vDirFromHit = vNearestCorner - hit.point;
                    vDirFromHit.Normalize();

                    Vector2 vPushedPosition = vNearestCorner + vDirFromHit * 0.2f;
                    
                    vCurrentDirection = vPushedPosition - vPos;
                    vCurrentDirection.Normalize();
                    vCourseCorrectedPosition = vPushedPosition;

                    debugLastHitOrigin = vPos;
                    debugLastHit = hit;

                    // If the new position also overlaps, just pick a new direction
                    var newOverlapped = StaticObject.GetOverlapped(vPos + (vCurrentDirection * fAheadDist));
                    if (newOverlapped != null)
                    {
                        vCurrentDirection = GetNewWanderDirection();
                    }
                    
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

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (state != State.Idle && state != State.Stunned)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(GetOffsetPosition() + new Vector2(0.0f, 0.05f), GetOffsetPosition() + (vCurrentDirection * GetPathfindingAheadDistance()) + new Vector2(0.0f, 0.05f));
        }

        if (debugLastHit.point != Vector2.zero)
        {
            Vector2 vNearestCorner = GetNearestColliderPoint(debugLastHit);
            Vector2 vDirFromHit = vNearestCorner - debugLastHit.point;
            vDirFromHit.Normalize();

            Vector2 vPushedPosition = vNearestCorner + vDirFromHit * 0.5f;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(debugLastHitOrigin, debugLastHit.point);

            Gizmos.color = new Color(1.0f, 0.65f, 0.0f);
            Gizmos.DrawLine(debugLastHit.point, vNearestCorner);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(vNearestCorner, vPushedPosition);
        }

        if (state == State.Fleeing)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(GetOffsetPosition(), vFleeTarget);
        }
    }
}

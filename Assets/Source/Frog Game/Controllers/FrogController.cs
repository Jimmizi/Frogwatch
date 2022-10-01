using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class FrogController : HumanoidController
{
    public enum State
    {
        Free,
        Carried,
        InPond
    }



    private FrogSystemVars tuning;
    //public float MinTimeBetweenHops = 1.0f;
    //public float MaxTimeBetweenHops = 10.0f;

    //public float NearbyAvoidRadius = 1.0f;

    //public float HopDistance = 1.0f;
    //public float HopMovementSpeed = 5.0f;

    private float nextHopInterval;
    private float hopTimer;

    private State state;
    private bool performingHop;


    //public Vector2 DirectionTest = new();
    //public float DotToPlayer = 0.0f;

    // Start is called before the first frame update
    protected override void Start()
    {
        tuning = Service.Vars<FrogSystemVars>();
        ResetTimer();
        state = State.Free;

        base.Start();
    }

    void ResetTimer()
    {
        nextHopInterval = GetNextHopInterval();
        hopTimer = 0.0f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (!performingHop)
        {
            hopTimer += Time.deltaTime;
            if (hopTimer >= nextHopInterval)
            {
                TryPerformHop();
                ResetTimer();
            }
        }

        base.Update();
    }

    protected override void FixedUpdate()
    {
    }

    void TryPerformHop()
    {
        if (state != State.Free)
        {
            return;
        }

        Vector2 dir = GetHopDirection();
        Vector2 vPos = transform.position;

        StartCoroutine(PerformHop(vPos + (dir * tuning.HopDistance)));
    }

    IEnumerator PerformHop(Vector2 vNewPos)
    {
        performingHop = true;
        m_animator.SetBool("IsHopping", true);

        Vector2 vOriginalPos = m_rigidbody.position;


        float fTime = 0.0f;

        while (fTime <= 1.0f)
        {
            m_rigidbody.position = Easer.EaseVector2(tuning.MovementEaser, vOriginalPos, vNewPos, fTime);
            
            fTime += Time.deltaTime * tuning.HopMovementSpeed;

            if (state != State.Free)
            {
                break;
            }
            
            yield return new WaitForSeconds(Time.deltaTime);
        }

        m_rigidbody.position = vNewPos;

        m_animator.SetBool("IsHopping", false);
        performingHop = false;
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

        List<BoxCollider2D> frogBounds = Service.Vars<FrogSystemVars>().FrogMovementBounds;

        for (int i = 0; i < NumDirectionsToScore; ++i)
        {
            Vector2 vRandomDir = Random.insideUnitCircle.normalized;
            Vector2 vThisPos = transform.position;

            if (directions.ContainsKey(vRandomDir))
            {
                continue;
            }

            Vector2 vPossibleNewPosition = vThisPos + (vRandomDir * tuning.HopDistance);
            bool bInvalidPosition = false;

            foreach (var bound in frogBounds)
            {
                if (!bound.OverlapPoint(vPossibleNewPosition))
                {
                    bInvalidPosition = true;
                    break;
                }
            }

            if (bInvalidPosition)
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

    void OnDrawGizmosSelected()
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

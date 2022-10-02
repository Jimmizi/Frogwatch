using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class FrogController : HumanoidController
{
    public static int GetNumberOfFrogs()
    {
        return FrogList.Count;
    }
    public static int GetNumberOfFrogsInPonds()
    {
        int iCount = 0;
        foreach (var frog in FrogList)
        {
            if (frog.state == State.InPond)
            {
                ++iCount;
            }
        }

        return iCount;
    }
    public static int GetNumberOfCarriedFrogs()
    {
        int iCount = 0;
        foreach (var frog in FrogList)
        {
            if (frog.state == State.Carried)
            {
                ++iCount;
            }
        }

        return iCount;
    }
    public static int NumFrogsTaken = 0;



    public enum State
    {
        OnSpawn,
        Idle,
        Hopping,
        Carried,
        Thrown,
        InPond
    }

    public static List<FrogController> FrogList = new();

    public SpriteRenderer FrogHeldBubblesRenderer;
    public SpriteRenderer FrogHeldHeartsRenderer;
    public Animator FrogLandAnimator;
    private bool bAbortFadeInBubbles = false;
    private bool bAbortFadeOutBubbles = false;
    private bool bAbortFadeInHearts = false;
    private bool bAbortFadeOutHearts = false;

    private float nextHopInterval;
    private float hopTimer;
    private float fThrowTime;
    private bool bFirstThrowSectionDone;
    private float fDropTimeAccelHops = 0.0f;
    private float escapeTestTimer = 0.0f;

    public Vector2 SpawnPosition;

    private bool bPerformingCarriedPtfxFadeInBubbles = false;
    private bool bPerformingCarriedPtfxFadeOutBubbles = false;

    private bool bPerformingCarriedPtfxFadeInHearts = false;
    private bool bPerformingCarriedPtfxFadeOutHearts = false;

    public GameObject exclamationMark;

    private PondDropArea pondFrogIsIn;

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
        state = State.OnSpawn;

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
        FrogLandAnimator.SetTrigger("TriggerAbort"); // triggers mud splash abort in case of something picking up frog quickly
        if (bHeldByWitch)
        {
            if (exclamationMark != null)
            {
                exclamationMark.SetActive(true);
            }

            m_animator.SetBool("HeldByWitch", true);
            StartCoroutine(DoBubblesFadeIn());
        }
        else
        {
            StartCoroutine(DoHeartsFadeIn());
        }
    }

    public void SetDropped(bool bDueToStunned = false)
    {
        m_animator.SetBool("IsCarried", false);
        m_animator.SetBool("HeldByWitch", false);

        if (exclamationMark != null)
        {
            exclamationMark.SetActive(false);
        }
        
        Vector2 vDir = (bDueToStunned ? HumanoidController.Player.InputDirection : GetHopDirection()) * 0.5f;

        StartCoroutine(PerformThrown(vDir, true));
        StartCoroutine(DoBubblesFadeOut());
        StartCoroutine(DoHeartsFadeOut());

    }

    public void SetThrown(Vector2 dir)
    {
        StartCoroutine(PerformThrown(dir));
        StartCoroutine(DoBubblesFadeOut());
        StartCoroutine(DoHeartsFadeOut());
    }

    public bool CanPickup()
    {
        return state == State.Idle || state == State.Hopping;
    }

    public bool ShouldDrawInFrontDuringThrow()
    {
        return bFirstThrowSectionDone && fThrowTime < 0.45f;
    }

    public override void OnJustSpawned()
    {
        ExternalSetPosition(new Vector2(SpawnPosition.x, 9.0f));
        StartCoroutine(DoSpawning());

        base.OnJustSpawned();
    }

    // Update is called once per frame
    protected new void Update()
    {
        if (fDropTimeAccelHops >= 0.0f)
        {
            fDropTimeAccelHops -= Time.deltaTime;
        }

        // Fallback incase something doesn't clean this up
        if (state != State.Carried)
        {
            if (!bPerformingCarriedPtfxFadeInBubbles && !bPerformingCarriedPtfxFadeOutBubbles && FrogHeldBubblesRenderer.color.a > 0.0f)
            {
                Color col = FrogHeldBubblesRenderer.color;
                col.a = 0.0f;
                FrogHeldBubblesRenderer.color = col;
            }
            if (!bPerformingCarriedPtfxFadeInHearts && !bPerformingCarriedPtfxFadeOutHearts && FrogHeldHeartsRenderer.color.a > 0.0f)
            {
                Color col = FrogHeldHeartsRenderer.color;
                col.a = 0.0f;
                FrogHeldHeartsRenderer.color = col;
            }
        }

        switch (state)
        {
            case State.OnSpawn:
                ProcessSpawning();
                break;
            case State.Idle:
            {
                TryContainFrogs();

                hopTimer += Time.deltaTime * (fDropTimeAccelHops > 0.0f ? 4.0f : 1.0f);
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
                ProcessInPond();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ProcessSpawning()
    {

    }

    IEnumerator DoSpawning()
    {
        float fTime = 0.0f;
        Vector2 vOriginalPos = m_rigidbody.position;
        float fBaseScale = 0.2f;

        transform.localScale = new Vector3(fBaseScale, fBaseScale, 1.0f);

        float fNextScale = fBaseScale;

        while (fTime <= 1.0f)
        {
            fNextScale += (1.0f - fBaseScale) * Time.deltaTime;
            fNextScale = Mathf.Clamp(fNextScale, 0.0f, 1.0f);

            transform.localScale = new Vector3(fNextScale, fNextScale, 1.0f);

            m_rigidbody.position = Easer.EaseVector2(GetVars().SpawnEaser, vOriginalPos, SpawnPosition, fTime);
            fTime += Time.deltaTime;
            
            yield return new WaitForSeconds(Time.deltaTime);
        }

        // If we want camera shake - also add on camera
        //if (Vector2.Distance(m_rigidbody.position, Player.GetOffsetPosition()) < 2.0f)
        //{
        //    Camera.main.GetComponent<ProCamera2DShake>().Shake(0.5f, new Vector2(Random.Range(0.05f, 0.2f), Random.Range(0.05f, 0.2f)));
        //}

        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        Service.Get<AudioSystem>().PlayEvent(AudioEvent.FrogLand, transform.position);
        FrogLandAnimator.SetTrigger("TriggerLand");
        state = State.Idle;
    }

    void ProcessInPond()
    {
        // Shouldn't hit but just in case
        if (pondFrogIsIn == null)
        {
            state = State.Idle;
            return;
        }

        escapeTestTimer += Time.deltaTime;

        if (escapeTestTimer >= GetVars().FrogEscapeTestInterval)
        {
            escapeTestTimer = 0.0f;

            FrogSystemVars vars = GetVars();
            float fChanceToEscape = vars.BaseEscapeChance;

            // i starts at 1, don't include self as an extra frog
            for (int i = 1; i < pondFrogIsIn.GetNumFrogsInPond(); ++i)
            {
                // Add extra chance per frog so that we get more randomness
                float fExtraChance = Random.Range(vars.AdditionalEscapeChancePerExtraFrogMin, vars.AdditionalEscapeChancePerExtraFrogMax);
                fChanceToEscape += fExtraChance;
            }
            
            if (Random.Range(0.0f, 100.0f) < fChanceToEscape)
            {
                pondFrogIsIn.RemoveFrog(this);
                m_animator.SetBool("InPond", false);
                
                TryPerformHop(true);
                pondFrogIsIn = null;
            }
            else if(Random.Range(0.0f, 100.0f) < 20.0f)
            {
                pondFrogIsIn.RemoveFrog(this);
                m_animator.SetBool("InPond", false);
                TryPerformHop(false, true);
            }
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

    void TryPerformHop(bool bToEscapePond = false, bool bZeroDirection = false)
    {
        float fMod = (bToEscapePond ? 2.0f : 1.0f);
        if (pondFrogIsIn != null && pondFrogIsIn.IsLarge)
        {
            fMod *= 2.0f;
        }

        Vector2 dir = !bZeroDirection ? GetHopDirection(bToEscapePond, fMod) : Vector2.zero;
        Vector2 vPos = transform.position;
        
        state = State.Hopping;
        StartCoroutine(PerformHop(vPos + (dir * GetVars().HopDistance * fMod)));
    }

    void AssignToPond(PondDropArea newPond)
    {
        newPond.AddFrog(this);
        pondFrogIsIn = newPond;

        state = State.InPond;

        Service.Get<AudioSystem>().PlayEvent(AudioEvent.FrogSplash, transform.position);
        FrogLandAnimator.SetTrigger("TriggerSplash");
        m_animator.SetBool("InPond", true);
        escapeTestTimer = 0.0f;
    }

    IEnumerator PerformThrown(Vector2 vDir, bool bAccelHops = false)
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
        ResetTimer();
        
        PondDropArea pondDroppedOn = PondDropArea.GetOverlapped(GetOffsetPosition());
        if (pondDroppedOn != null)
        {
            AssignToPond(pondDroppedOn);

            GameStats.RecordFrogThrownIntoPond();
        }
        else
        {
            state = State.Idle;

            if (bAccelHops)
            {
                fDropTimeAccelHops = 4.0f;
            }

            Service.Get<AudioSystem>().PlayEvent(AudioEvent.FrogLand, transform.position);
            FrogLandAnimator.SetTrigger("TriggerLand");
        }

        m_animator.SetBool("IsCarried", false);
        m_animator.SetBool("IsHopping", false);
        m_animator.SetBool("HeldByWitch", false);
    }

    IEnumerator PerformHop(Vector2 vNewPos)
    {
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

        bool bAssignedToPond = false;

        if (!bAbort)
        {
            m_rigidbody.position = vNewPos;
            state = State.Idle;

            if (GetVars().AllowFrogsToAmbientlyHopIntoPonds)
            {
                PondDropArea pondDroppedOn = PondDropArea.GetOverlapped(GetOffsetPosition());
                if (pondDroppedOn != null)
                {
                    AssignToPond(pondDroppedOn);
                    bAssignedToPond = true;
                }
            }

            if (!bAssignedToPond)
            {
                Service.Get<AudioSystem>().PlayEvent(AudioEvent.FrogLand, transform.position);
                FrogLandAnimator.SetTrigger("TriggerLand");
            }
        }
        
        m_animator.SetBool("IsHopping", false);
    }

    private float GetNextHopInterval()
    {
        return Random.Range(GetVars().MinTimeBetweenHops, GetVars().MaxTimeBetweenHops);
    }
    
    private Vector2 GetHopDirection(bool isEscapingPond = false, float fHopMod = 1.0f)
    {
        float hopDist = GetVars().HopDistance * fHopMod;
        
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

            // Don't hop out of bounds
            Vector2 vPossibleNewPosition = vThisPos + (vRandomDir * hopDist);
            if (!frogBounds.OverlapPoint(vPossibleNewPosition))
            {
                continue;
            }

            PondDropArea pondDroppedOn = PondDropArea.GetOverlapped(vPossibleNewPosition);

            // Don't hop into static objects
            if (StaticObject.GetOverlapped(vPossibleNewPosition) != null)
            {
                // Potentially allow hopping onto this static object to be okay 
                if (!GetVars().AllowFrogsToAmbientlyHopIntoPonds || pondDroppedOn == null)
                {
                    continue;
                }
            }

            // If not allowing ambient hopping into ponds or we're escaping from one, don't pick positions in ponds
            if (!GetVars().AllowFrogsToAmbientlyHopIntoPonds || isEscapingPond)
            {
                if (pondDroppedOn != null)
                {
                    continue;
                }
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
                fFacingDot += isFrog ? fDot * 0.05f : -fDot * (extremeEdge ? 0.1f : 0.5f);
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
    
    IEnumerator DoBubblesFadeOut()
    {
        void MinusAlpha(float a)
        {
            Color col = FrogHeldBubblesRenderer.color;
            col.a -= a;
            FrogHeldBubblesRenderer.color = col;
        }

        bPerformingCarriedPtfxFadeOutBubbles = true;

        bAbortFadeOutBubbles = false;
        bAbortFadeInBubbles = true;

        while (FrogHeldBubblesRenderer.color.a > 0.0f)
        {
            if (bAbortFadeOutBubbles)
            {
                break;
            }

            // Fade out over half a second
            MinusAlpha((Time.deltaTime) * 2);

            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (!bAbortFadeOutBubbles)
        {
            Color col = FrogHeldBubblesRenderer.color;
            col.a = 0.0f;
            FrogHeldBubblesRenderer.color = col;
        }

        bAbortFadeInBubbles = false;

        bPerformingCarriedPtfxFadeOutBubbles = false;

        yield return null;
    }

    IEnumerator DoBubblesFadeIn()
    {
        void AddAlpha(float a)
        {
            Color col = FrogHeldBubblesRenderer.color;
            col.a += a;
            FrogHeldBubblesRenderer.color = col;
        }

        bPerformingCarriedPtfxFadeInBubbles = true;

        bAbortFadeInBubbles = false;

        bAbortFadeOutBubbles = true;
        bAbortFadeInHearts = true;
        bAbortFadeOutHearts = true;

        while (FrogHeldBubblesRenderer.color.a < 1.0f)
        {
            if (bAbortFadeInBubbles)
            {
                break;
            }

            // Fade in over half a second
            AddAlpha((Time.deltaTime) * 2);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (!bAbortFadeInBubbles)
        {
            Color col = FrogHeldBubblesRenderer.color;
            col.a = 1.0f;
            FrogHeldBubblesRenderer.color = col;
        }

        bAbortFadeOutBubbles = false;

        bPerformingCarriedPtfxFadeInBubbles = false;

        yield return null;
    }


    IEnumerator DoHeartsFadeOut()
    {
        void MinusAlpha(float a)
        {
            Color col = FrogHeldHeartsRenderer.color;
            col.a -= a;
            FrogHeldHeartsRenderer.color = col;
        }

        bPerformingCarriedPtfxFadeOutHearts = true;

        bAbortFadeOutHearts = false;
        bAbortFadeInHearts = true;

        while (FrogHeldHeartsRenderer.color.a > 0.0f)
        {
            if (bAbortFadeOutHearts)
            {
                break;
            }

            // Fade out over half a second
            MinusAlpha((Time.deltaTime) * 2);

            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (!bAbortFadeOutHearts)
        {
            Color col = FrogHeldHeartsRenderer.color;
            col.a = 0.0f;
            FrogHeldHeartsRenderer.color = col;
        }

        bAbortFadeInHearts = false;

        bPerformingCarriedPtfxFadeOutHearts = false;

        yield return null;
    }

    IEnumerator DoHeartsFadeIn()
    {
        void AddAlpha(float a)
        {
            Color col = FrogHeldHeartsRenderer.color;
            col.a += a;
            FrogHeldHeartsRenderer.color = col;
        }

        bPerformingCarriedPtfxFadeInHearts = true;

        bAbortFadeInHearts = false;
        bAbortFadeOutHearts = true;

        while (FrogHeldHeartsRenderer.color.a < 1.0f)
        {
            if (bAbortFadeInHearts)
            {
                break;
            }

            // Fade in over half a second
            AddAlpha((Time.deltaTime) * 2);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        if (!bAbortFadeInHearts)
        {
            Color col = FrogHeldHeartsRenderer.color;
            col.a = 1.0f;
            FrogHeldHeartsRenderer.color = col;
        }

        bAbortFadeOutHearts = false;

        bPerformingCarriedPtfxFadeInHearts = false;

        yield return null;
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

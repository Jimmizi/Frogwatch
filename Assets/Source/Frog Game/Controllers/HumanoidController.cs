using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

[RequireComponent(typeof(ZSortObject))]
public class HumanoidController : MonoBehaviour
{
    public static List<HumanoidController> Controllers = new();
    public static PlayerController Player = null;

    public Animator OnSpawnedAnimator;

    public EaserEase FrogCarryFloatEase;
    public float ControllerSpeed = 5.0f;
    public float DashSpeed = 7.5f;
    public float PickupDistance = 1.0f;
    public float DashTime = 0.05f;
    public float DashLogicalTime = 0.35f;
    public Vector2 FrogCarryOffsetStart = new();
    public Vector2 FrogCarryOffsetEnd = new();
    public float CarryFrogAnimSpeed = 0.5f;

    public float InteractCooldown = 1.0f;
    public float DashCooldown = 3.0f;

    public Vector2 InputDirection = new();
    public Vector2 FacingDirection = new();
    protected bool JustPressedInteract;
    protected bool JustPressedDash;
    protected Rigidbody2D m_rigidbody;
    protected Animator m_animator;
    protected Vector2 BaseOffset = new();

    [HideInInspector]
    public ZSortObject ZSort = null;

    [HideInInspector]
    public FrogController FrogCarrying = null;

    private string IsWalkingAnimVarName = "IsWalking";
    private string IsCarryingAnimVarName = "IsCarrying";

    private float carryFrogTimer = 0.0f;
    private bool carryFrogTimeFlipped = false;

    private float interactCooldownTimer = 0.0f;
    public float DashCooldownTimer = 0.0f;
    protected float dashIsAliveTimer = 0.0f;
    
    public bool IsCarryingFrog => FrogCarrying != null;
    public bool IsDashing => dashIsAliveTimer > 0.0f;

    public static Vector2 GetPlayerPosition()
    {
        return Player.GetOffsetPosition();
    }
    public Vector2 GetOffsetPosition()
    {
        return new Vector2(transform.position.x, transform.position.y) + BaseOffset;
    }

    public void ExternalSetPosition(Vector2 vPos)
    {
        m_rigidbody.position = vPos - BaseOffset;
    }

    public virtual void OnJustSpawned()
    {
        if (OnSpawnedAnimator != null)
        {
            //OnSpawnedAnimator.SetTrigger("Start");
        }
    }
    
    protected virtual void Start()
    {
        
    }

    protected virtual void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        ZSort = GetComponent<ZSortObject>();
        Controllers.Add(this);

        BaseOffset = GetComponent<ZSortObject>().GroundContactPosition;
    }

    protected virtual void OnDestroy()
    {
        Controllers.Remove(this);
    }
    
    protected bool IsAnimWalking()
    {
        return m_animator.GetBool(IsWalkingAnimVarName);
    }
    protected bool IsAnimCarrying()
    {
        return m_animator.GetBool(IsCarryingAnimVarName);
    }
    protected void SetAnimWalking(bool val)
    {
        m_animator.SetBool(IsWalkingAnimVarName, val);
    }
    protected void SetAnimCarrying(bool val)
    {
        m_animator.SetBool(IsCarryingAnimVarName, val);
    }

    public bool CanInteract()
    {
        return interactCooldownTimer <= 0.0f;
    }

    public bool CanDash()
    {
        return DashCooldownTimer <= 0.0f && !IsDashing;
    }

    public FrogSystemVars GetVars()
    {
        return Service.Vars<FrogSystemVars>();
    }

    protected virtual float GetCurrentSpeedMult()
    {
        return ControllerSpeed;
    }

    protected virtual bool CanLeaveBounds()
    {
        return false;
    }

    private void KeepInBounds()
    {
        if (CanLeaveBounds())
        {
            return;
        }

        var bounds = GetVars().FrogMovementBounds;
        if (bounds != null)
        {
            Vector2 vPos = m_rigidbody.position;
            var futurePos = vPos + (InputDirection * GetCurrentSpeedMult() * Time.deltaTime);
            if (!bounds.OverlapPoint(futurePos))
            {
                InputDirection = Vector2.zero;
            }

            if (!bounds.OverlapPoint(vPos))
            {
                Vector2 vClosestPoint = bounds.ClosestPoint(vPos);
                Vector2 vDirToCenter = bounds.offset - vClosestPoint;
                vDirToCenter.Normalize();

                vClosestPoint += vDirToCenter;
                m_rigidbody.position = vClosestPoint;
            }
        }
    }

    protected void SetCarryingFrog(FrogController frog, bool bByWitch = false)
    {
        Service.Get<AudioSystem>().PlayEvent(bByWitch ? AudioEvent.WitchPickedFrog : AudioEvent.PickUpFrog, transform.position);
        frog.SetCarried(bByWitch);
        FrogCarrying = frog;

        SetAnimCarrying(true);

        carryFrogTimer = 0.0f;
        carryFrogTimeFlipped = false;
    }

    protected void DropCarriedFrog(bool bDueToStunned = false)
    {
        if (FrogCarrying != null)
        {
            SetAnimCarrying(false);
            FrogCarrying.SetDropped(bDueToStunned);
            FrogCarrying = null;
            carryFrogTimer = 0.0f;
            carryFrogTimeFlipped = false;
        }
    }

    protected virtual void Update()
    {
        if (StatsScreen.IsOnStatsScreen)
        {
            return;
        }

        KeepInBounds();

        Vector2 addition = InputDirection * GetCurrentSpeedMult() * Time.deltaTime;
        m_rigidbody.position += addition;

        if (ZSort.IsPlayer)
        {
            GameStats.AddDistanceMoved(addition.magnitude);
        }

        SetAnimWalking(InputDirection.x != 0.0f || InputDirection.y != 0.0f);
        
        if (JustPressedInteract)
        {
            Debug.Log("Just pressed interact.");
            if (CanInteract())
            {
                //interactCooldownTimer = InteractCooldown; // Remove cooldown for now

                if (!IsCarryingFrog)
                {
                    var frogFound = GetBestFrogWithinPickupDistance();
                    if (frogFound != null)
                    {
                        SetCarryingFrog(frogFound);
                    }
                }
                else
                {
                    Vector2 vDirection = FacingDirection.x == 0.0f && FacingDirection.y == 0.0f ? Random.insideUnitCircle.normalized : FacingDirection;

                    if (FrogCarrying.CanThrowInDirection(vDirection))
                    {
                        Service.Get<AudioSystem>().PlayEvent(AudioEvent.ThrowFrog, transform.position);
                        FrogCarrying.SetThrown(vDirection);
                        FrogCarrying = null;

                        SetAnimCarrying(false);
                        carryFrogTimer = 0.0f;
                        carryFrogTimeFlipped = false;
                    }
                }
            }
        }
        else if(!CanInteract())
        {
            interactCooldownTimer -= Time.deltaTime;
        }

        if (JustPressedDash)
        {
            if (CanDash() && FacingDirection != Vector2.zero)
            {
                StartCoroutine(DoDash(FacingDirection));
            }
        }
        // If can't dash and we're done with our alive timer, then we can start the cooldown going down
        else if (!CanDash() && dashIsAliveTimer <= 0.0f)
        {
            // Done in player
            //DashCooldownTimer -= Time.deltaTime;

            // Done in player
            //if (DashCooldownTimer <= 0.0f)
            //{
            //    OnDashRecharged();
            //}
        }

        // Done in player
        //if (dashIsAliveTimer > 0.0f)
        //{
        //    dashIsAliveTimer -= Time.deltaTime;
        //}
    }

    protected virtual void OnDashStart() {}
    protected virtual void OnDashRecharged() {}

    IEnumerator DoDash(Vector2 dir)
    {
        // Done in player
        //dashIsAliveTimer = DashLogicalTime;
        //DashCooldownTimer = DashCooldown;

        OnDashStart();

        float fTime = 0.0f;

        var bounds = GetVars().FrogMovementBounds;
        while (fTime < DashTime)
        {
            fTime += Time.deltaTime;

            Vector2 vAddition = dir * DashSpeed * Time.deltaTime;

            if (bounds != null)
            {
                // Stop a dash if it would put us out of bounds
                var futurePos = m_rigidbody.position + vAddition;
                if (!bounds.OverlapPoint(futurePos))
                {
                    break;
                }
            }

            if (ZSort.IsPlayer)
            {
                GameStats.AddDistanceMoved(vAddition.magnitude);
            }

            m_rigidbody.position += vAddition;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    protected virtual void FixedUpdate()
    {
        // Frog positioning when carrying
        if (FrogCarrying != null)
        {
            if (!carryFrogTimeFlipped)
            {
                carryFrogTimer += Time.deltaTime * CarryFrogAnimSpeed;
                if (carryFrogTimer >= 1.0f)
                {
                    carryFrogTimeFlipped = true;
                }
            }
            else
            {
                carryFrogTimer -= Time.deltaTime * CarryFrogAnimSpeed;
                if (carryFrogTimer < 0.0f)
                {
                    carryFrogTimeFlipped = false;
                }
            }

            Vector2 vThisPos = transform.position;
            Vector2 vStartPos = vThisPos + FrogCarryOffsetStart;
            Vector2 vEndPos = vThisPos + FrogCarryOffsetEnd;

            FrogCarrying.transform.position = Easer.EaseVector2(FrogCarryFloatEase, vStartPos, vEndPos, carryFrogTimer);
        }
    }

    public static List<HumanoidController> GetControllersInArea(Vector2 vec, float rad)
    {
        // Dirty

        List<HumanoidController> retControllers = new();
        foreach (var c in Controllers)
        {
            if (Vector2.Distance(c.transform.position, vec) < rad)
            {
                retControllers.Add(c);
            }
        }

        return retControllers;
    }

    public FrogController GetBestFrogWithinPickupDistance()
    {
        return GetFrogToPickup(PickupDistance);
    }

    public FrogController GetBestFrogToHunt()
    {
        return GetFrogToPickup(GetVars().WitchLookForFrogRange);
    }

    public FrogController GetFrogToPickup(float range)
    {
        // Also dirty

        Vector2 vQueryPosition = GetOffsetPosition();
        
        SortedDictionary<float, FrogController> nearbyFrogs = new();
        foreach (var c in Controllers)
        {
            FrogController frog = c as FrogController;
            if (frog != null)
            {
                if (!frog.CanPickup())
                {
                    continue;
                }

                float dist = Vector2.Distance(frog.GetOffsetPosition(), vQueryPosition);

                // Don't worry about frogs equal distance
                if (dist < range && !nearbyFrogs.ContainsKey(dist))
                {
                    nearbyFrogs.Add(dist, frog);
                }
            }
        }
        
        return nearbyFrogs.Count > 0 ? nearbyFrogs.First().Value : null;
    }

    protected virtual void OnDrawGizmos()
    {
        if (IsDashing)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(GetOffsetPosition(), 0.25f);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(transform.position.x + FrogCarryOffsetStart.x, transform.position.y + FrogCarryOffsetStart.y, 0.0f), new Vector3(0.025f, 0.025f, 0.025f));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(transform.position.x + FrogCarryOffsetEnd.x, transform.position.y + FrogCarryOffsetEnd.y, 0.0f), new Vector3(0.025f, 0.025f, 0.025f));


        Gizmos.color = new Color(1f, 0.1215686f, 0.01568628f, 0.25f);
        Gizmos.DrawSphere(GetOffsetPosition(), PickupDistance);
    }
}

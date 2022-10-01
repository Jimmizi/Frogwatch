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

    public EaserEase FrogCarryFloatEase;
    public float ControllerSpeed = 5.0f;
    public float PickupDistance = 1.0f;
    public Vector2 FrogCarryOffsetStart = new();
    public Vector2 FrogCarryOffsetEnd = new();
    public float CarryFrogAnimSpeed = 0.5f;

    public float InteractCooldown = 1.0f;

    protected Vector2 InputDirection = new();
    protected bool JustPressedInteract;
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


    public bool IsCarryingFrog => FrogCarrying != null;

    public static Vector2 GetPlayerPosition()
    {
        return Player.GetOffsetPosition();
    }
    public Vector2 GetOffsetPosition()
    {
        return new Vector2(transform.position.x, transform.position.y) + BaseOffset;
    }

    // Start is called before the first frame update
    protected virtual void Start()
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

    // Update is called once per frame
    protected virtual void Update()
    {
        m_rigidbody.position += InputDirection * ControllerSpeed * Time.deltaTime;
        SetAnimWalking(InputDirection.x != 0.0f || InputDirection.y != 0.0f);
        
        if (JustPressedInteract)
        {
            Debug.Log("Just pressed interact.");
            if (CanInteract())
            {
                //interactCooldownTimer = InteractCooldown;

                if (!IsCarryingFrog)
                {
                    FrogCarrying = GetFrogToPickup();
                    if (FrogCarrying != null)
                    {
                        FrogCarrying.SetCarried();
                        SetAnimCarrying(true);

                        carryFrogTimer = 0.0f;
                        carryFrogTimeFlipped = false;
                    }
                }
                else
                {
                    FrogCarrying.SetThrown(InputDirection.x == 0.0f && InputDirection.y == 0.0f ? Random.insideUnitCircle.normalized : InputDirection);
                    FrogCarrying = null;

                    SetAnimCarrying(false);
                    carryFrogTimer = 0.0f;
                    carryFrogTimeFlipped = false;
                }
            }
        }
        else if(!CanInteract())
        {
            interactCooldownTimer -= Time.deltaTime;
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

    public FrogController GetFrogToPickup()
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
                if (dist < PickupDistance && !nearbyFrogs.ContainsKey(dist))
                {
                    nearbyFrogs.Add(dist, frog);
                }
            }
        }
        
        return nearbyFrogs.Count > 0 ? nearbyFrogs.First().Value : null;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(transform.position.x + FrogCarryOffsetStart.x, transform.position.y + FrogCarryOffsetStart.y, 0.0f), new Vector3(0.1f, 0.1f, 0.1f));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(transform.position.x + FrogCarryOffsetEnd.x, transform.position.y + FrogCarryOffsetEnd.y, 0.0f), new Vector3(0.1f, 0.1f, 0.1f));
    }
}

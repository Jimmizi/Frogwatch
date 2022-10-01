using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidController : MonoBehaviour
{
    public static List<HumanoidController> Controllers = new();

    public float ControllerSpeed = 5.0f;

    protected Vector2 InputDirection = new();
    protected Rigidbody2D m_rigidbody;
    protected Animator m_animator;

    private string IsWalkingAnimVarName = "IsWalking";
    private string IsCarryingAnimVarName = "IsCarrying";

    // Start is called before the first frame update
    protected virtual void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
        Controllers.Add(this);
    }

    protected virtual void OnDestroy()
    {
        Controllers.Remove(this);
    }

    protected virtual void Update()
    {

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

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        m_rigidbody.position += InputDirection * ControllerSpeed * Time.deltaTime;

        SetAnimWalking(InputDirection.x != 0.0f || InputDirection.y != 0.0f);
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
}

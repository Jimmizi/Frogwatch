using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidController : MonoBehaviour
{
    public float ControllerSpeed = 5.0f;

    protected Vector2 InputDirection = new();
    protected Rigidbody2D m_rigidbody;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected virtual void FixedUpdate()
    {
        m_rigidbody.position += InputDirection * ControllerSpeed * Time.deltaTime;
    }
}
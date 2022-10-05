using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDirection : MonoBehaviour
{
    Animator animator;
    Transform distanceTransform;

    float markerMoveSpeed = 3.0f;
    float normalDistance = 0.0f;    
    float markDistance => Service.Vars<FrogSystemVars>().ThrownDistance;

    public bool isMark
    {
        get { return animator.GetBool("IsMark"); }
        set { animator.SetBool("IsMark", value); }
    }

    public Vector2 direction { get; set; }

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        distanceTransform = transform.GetChild(0);

        normalDistance = distanceTransform.localPosition.x;
    }

    void Update()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0,0,angle);

        float targetDistance = isMark ? markDistance : normalDistance;
        Vector3 position = distanceTransform.transform.localPosition;
        position.x = Mathf.Lerp(position.x, targetDistance, Time.deltaTime * markerMoveSpeed);
        distanceTransform.transform.localPosition = position;
    }
}

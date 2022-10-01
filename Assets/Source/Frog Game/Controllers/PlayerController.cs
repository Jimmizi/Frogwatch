using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : HumanoidController
{
    // Start is called before the first frame update
    protected virtual void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        float fHorizontal = Input.GetAxis("Horizontal");
        float fVertical = Input.GetAxis("Vertical");

        InputDirection = new Vector2(fHorizontal, fVertical);

        base.FixedUpdate();
    }
}

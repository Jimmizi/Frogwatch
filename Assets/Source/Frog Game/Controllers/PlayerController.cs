using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : HumanoidController
{
    // Start is called before the first frame update
    protected virtual void Start()
    {
        HumanoidController.Player = this;

        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        float fHorizontal = Input.GetAxis("Horizontal");
        float fVertical = Input.GetAxis("Vertical");
        bool bInteracted = Input.GetButtonDown("Interact");

        InputDirection = new Vector2(fHorizontal, fVertical);
        JustPressedInteract = bInteracted;

        base.Update();
    }

    
}

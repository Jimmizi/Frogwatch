using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HutongGames.PlayMaker;
using MyBox;

public class OutsideOfPlayMakerDebug : MonoBehaviour
{
    public bool WanderAround;
    [ConditionalField("WanderAround")] public float WanderDistance = 5;

    // Start is called before the first frame update
    void Start()
    {
        //string _value = FsmVariables.GlobalVariables.GetFsmString("globalString").Value;
        //PlayMakerFSM fsm;
       // fsm.FsmVariables
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

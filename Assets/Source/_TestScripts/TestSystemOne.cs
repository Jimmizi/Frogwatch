using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSystemOne : SystemObject
{
    public int Value = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AwakeService()
    {
        
    }

    public override void StartService()
    {
        var serviceTwoValue = Service.Get<TestSystemTwo>().Value;

        Debug.Log($"TestServiceOne - got value of serviceTwo: {serviceTwoValue}");
    }

    public override void UpdateService()
    {
        
    }

    public override void FixedUpdateService()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StaticObject))]
public class PondObject : MonoBehaviour
{
    public PondDropArea Area;

    // Start is called before the first frame update
    void Start()
    {
        Area = GetComponentInChildren<PondDropArea>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

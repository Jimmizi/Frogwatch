using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSystemVars : ServiceVars
{
    public GameObject FrogPrefab;
    public GameObject WitchPrefab;

    public List<GameObject> RareFrogPrefabs;
    public float ChanceForRareFrog = 10.0f;

    public bool AllowFrogsToAmbientlyHopIntoPonds = true;


    public BoxCollider2D FrogMovementBounds = new ();

    public float MinTimeBetweenHops = 1.0f;
    public float MaxTimeBetweenHops = 10.0f;

    public float NearbyAvoidRadius = 1.0f;
    public float SpawnNearbyAvoidRadius = 3.0f;

    public float HopDistance = 1.0f;
    public float HopMovementSpeed = 5.0f;

    public float ThrownSpeed = 0.75f;
    public float ThrownDistance = 1.0f;

    public float FrogEscapeTestInterval = 5.0f;
    public float BaseEscapeChance = 10.0f;
    public float AdditionalEscapeChancePerExtraFrogMin = 2.5f;
    public float AdditionalEscapeChancePerExtraFrogMax = 15.0f;

    public EaserEase MovementEaser;
    public EaserEase ThrownEaser;
    public EaserEase ThrownEaserSecondary;
    public EaserEase SpawnEaser;

    public float WitchStunnedMinTime = 4.0f;
    public float WitchStunnedMaxTime = 8.0f;
    public float WitchIdleMinTime = 1.0f;
    public float WitchIdleMaxTime = 4.0f;
    public float WitchWanderMinTime = 4.0f;
    public float WitchWanderMaxTime = 8.0f;

    public float WitchLookForFrogRange = 2.5f;

    public float WitchWanderSpeed = 0.25f;
    public float WitchChaseSpeed = 0.25f;
    public float WitchFleeSpeed = 0.25f;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSystemVars : ServiceVars
{
    public BoxCollider2D FrogMovementBounds = new ();

    public float MinTimeBetweenHops = 1.0f;
    public float MaxTimeBetweenHops = 10.0f;

    public float NearbyAvoidRadius = 1.0f;

    public float HopDistance = 1.0f;
    public float HopMovementSpeed = 5.0f;

    public float ThrownSpeed = 0.75f;
    public float ThrownDistance = 1.0f;

    public EaserEase MovementEaser;
    public EaserEase ThrownEaser;
    public EaserEase ThrownEaserSecondary;
}

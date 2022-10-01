using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class StaticObject : MonoBehaviour
{
    public static List<StaticObject> Geometry = new();

    private PolygonCollider2D m_collider;
    
    void Start()
    {
        m_collider = GetComponent<PolygonCollider2D>();
        Geometry.Add(this);
    }

    void OnDestroy()
    {
        Geometry.Remove(this);
    }

    public bool DoesPointOverlap(Vector2 point)
    {
        return m_collider.OverlapPoint(point);
    }
    
    public static StaticObject GetOverlapped(Vector2 point)
    {
        foreach (var so in Geometry)
        {
            if (so.m_collider.OverlapPoint(point))
            {
                return so;
            }
        }

        return null;
    }

}

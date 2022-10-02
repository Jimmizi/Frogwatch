using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

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

    public static StaticObject GetOverlappedIncrement(Vector2 point, Vector2 vReverseDir)
    {
        foreach (var so in Geometry)
        {
            for (int i = 0; i < 5; ++i)
            {
                if (so.m_collider.OverlapPoint(point + (vReverseDir * (i * 0.2f))))
                {
                    return so;
                }
            }
        }

        return null;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PondDropArea : MonoBehaviour
{
    public static List<BoxCollider2D> Areas = new();
    public BoxCollider2D CollectionBox;
    public BoxCollider2D DisplayBox;

    public List<Vector2> FrogSitOffsets = new();

    void Start()
    {
        CollectionBox = GetComponent<BoxCollider2D>();
        DisplayBox = GetComponentInChildren<BoxCollider2D>();
        Areas.Add(CollectionBox);

        float frogX = 0.32f;
        float frogY = 0.16f;

        Vector2 vWorldPos = new Vector2(DisplayBox.transform.position.x, DisplayBox.transform.position.y) + DisplayBox.offset;
        float halfHori = DisplayBox.size.x / 2;
        float halfVert = DisplayBox.size.y / 2;

        // Top left to bottom right solving
        float fStartX = halfHori + (frogX / 2);
        float fStartY = halfVert - (frogY / 2);

        //int iAmountX = 
    }

    void OnDestroy()
    {
        Areas.Remove(CollectionBox);
    }

    public static BoxCollider2D GetOverlapped(Vector2 point)
    {
        foreach (var box in Areas)
        {
            if (box.OverlapPoint(point))
            {
                return box;
            }
        }

        return null;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (DisplayBox != null)
            {
                Vector2 vWorldPos = new Vector2(DisplayBox.transform.position.x, DisplayBox.transform.position.y) + DisplayBox.offset;

                Gizmos.color = Color.gray;
                foreach (var offset in FrogSitOffsets)
                {
                    Gizmos.DrawSphere(vWorldPos + offset, 0.05f);
                }
            }
        }
    }
}

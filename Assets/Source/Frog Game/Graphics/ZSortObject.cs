using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZSortObject : MonoBehaviour
{
    [HideInInspector]
    public SpriteRenderer SpriteRend;

    public Vector2 GroundContactPosition = new();

    public Vector2 GetSortPosition()
    {
        return new Vector2(transform.position.x, transform.position.y) + GroundContactPosition;
    }

    // Start is called before the first frame update
    void Start()
    {
        SpriteRend = GetComponent<SpriteRenderer>();
        Service.Get<ZSortSystem>().ActiveFrogs.Add(this);
    }

    void OnDestroy()
    {
        Service.Get<ZSortSystem>().ActiveFrogs.Remove(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(transform.position.x + GroundContactPosition.x, transform.position.y + GroundContactPosition.y, 0.0f), new Vector3(0.1f, 0.1f, 0.1f));
    }
}

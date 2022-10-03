using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PondHighlighter : MonoBehaviour
{
    PlayerController _playerControllerl;
    protected PlayerController player
    {
        get
        {
            if (_playerControllerl == null)
            {
                _playerControllerl = FindObjectOfType<PlayerController>();
            }

            return _playerControllerl;
        }
    }


    Tilemap tilemap;
    float highlightStrength;

    public float glowSpeed = 4f;
    public float minHLAlpha = 0.15f;
    public float maxHLAlpha = 0.3f;
    
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }
    
    void Update()
    {
        float newAlpha = 0;
        float maxAlphaChange = Time.deltaTime * 0.3f;

        if (player != null && player.IsCarryingFrog)
        {
            float val = Mathf.Sin(Time.time * glowSpeed);
            newAlpha = Remap(val, -1, 1, minHLAlpha, maxHLAlpha);
        }

        float oldAlpha = tilemap.color.a;
        float alphaChange = newAlpha - oldAlpha;

        if (Mathf.Abs(alphaChange) > maxAlphaChange)
        {
            alphaChange = Mathf.Sign(alphaChange) * maxAlphaChange;
            newAlpha = oldAlpha + alphaChange;
        }

        Color color = tilemap.color;
        color.a = newAlpha;
        tilemap.color = color;
    }

    static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}

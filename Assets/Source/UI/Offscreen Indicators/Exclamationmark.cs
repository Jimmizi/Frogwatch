using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Exclamationmark : MonoBehaviour
{
    [SerializeField] Sprite sprite;
    [SerializeField] Sprite sprite2;
    [SerializeField] float speed = 1.6f;
    SpriteRenderer spriteRenderer;
    float resetTime = 0.2f;
    float timerCounter = 0;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Update()
    {
        timerCounter += Time.deltaTime;

        if (timerCounter > resetTime)
        {
            SpriteChange();
            timerCounter = 0;
        }
        float newScale = 0.7f + Mathf.PingPong(Time.time * speed , 0.5f);
        transform.localScale = new Vector3(newScale, newScale, newScale);
    }
    void SpriteChange()
    {
        if (spriteRenderer.sprite == sprite)
        {
            
            spriteRenderer.sprite = sprite2;

        } 
        else
        {
            spriteRenderer.sprite = sprite;
        }
    }
}

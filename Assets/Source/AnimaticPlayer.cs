using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AnimaticPlayer : MonoBehaviour
{    
    public Sprite[] frames;

    public string nextScene;

    RawImage image;
    float animationTime = 0.0f;
    float frameDuration = 0.125f;
    
    void Awake()
    {
        image = GetComponent<RawImage>();
    }

    void Update()
    {
        animationTime += Time.deltaTime;
        int frameIndex = (int) (animationTime / frameDuration);

        if (frameIndex < frames.Length)
        {
            image.texture = frames[frameIndex].texture;
        }
        else
        {
            OnAnimationEnd();
        }
    }

    void OnAnimationEnd()
    {
        // Load nex scene
        SceneManager.LoadScene(nextScene);
    }
}

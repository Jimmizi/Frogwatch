using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ToGameplayAfterVideo : MonoBehaviour
{
    public Animation Anim;
    public string NextScene; // Unity buildDoes not like SceneAsset

    private bool bStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        


    }

    // Update is called once per frame
    void Update()
    {
        if (!bStarted)
        {
            if (Anim.isPlaying)
            {
                bStarted = true;
            }
        } 
        else if (!Anim.isPlaying)
        {
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {        
        SceneManager.LoadScene(NextScene);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ToGameplayAfterVideo : MonoBehaviour
{
    public VideoPlayer Video;
    public SceneAsset NextScene;

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
            if (Video.frame != -1)
            {
                bStarted = true;
            }
        } 
        else if ((ulong)Video.frame >= Video.frameCount - 1)
        {
            Debug.Log("Video done");
            SceneManager.LoadScene(NextScene.name);
        }

    }
}

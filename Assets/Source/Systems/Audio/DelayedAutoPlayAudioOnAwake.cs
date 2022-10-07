using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedAutoPlayAudioOnAwake : MonoBehaviour
{
    [SerializeField]
    float delayTime;

    [SerializeField]
    AudioSource audioSource;
   
    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.PlayDelayed(delayTime);
        }

        Destroy(this);
    }
}

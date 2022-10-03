using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeControlMusic : MonoBehaviour
{    
    public AudioClip spookyMusic;

    AudioClip normalMusic;

    public float crossFadeTime = 0.75f;

    bool isSpookyOn = false;
        
    void Update()
    {
        bool shouldPlaySpooky = GetShouldPlaySpookyMusic();
        if (isSpookyOn != shouldPlaySpooky)
        {
            isSpookyOn = shouldPlaySpooky;
            if (isSpookyOn)
            {
                normalMusic = Service.Get<AudioSystem>().GetCurrentMusicClip();
            }
            PlayMusic(isSpookyOn ? spookyMusic : normalMusic);
        }
    }

    bool GetShouldPlaySpookyMusic()
    {
        return Service.Get<AudioSystem>().ShouldPlaySpookyMusic();
    }

    void PlayMusic(AudioClip music)
    {
        Service.Get<AudioSystem>().CrossFadeToMusic(music, crossFadeTime,0, false);
    }
}

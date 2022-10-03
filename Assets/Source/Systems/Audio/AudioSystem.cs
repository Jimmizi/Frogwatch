using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : SystemObjectWithVars<AudioSystemVars>
{
    private int lastOneShotIndex = 0;

    public override void AwakeService()
    {
    }

    public override void FixedUpdateService()
    {
    }

    public override void StartService()
    {
    }

    public override void UpdateService()
    {
    }

    public bool ShouldPlaySpookyMusic()
    {
        return HumanoidController.Player.IsInSpookZone();
    }

    public void PlayEvent(AudioEvent audioEvent, Vector3 position)
    {
        AudioClip clip = GetVars().FindAudioEventClip(audioEvent);
        if (clip != null)
        {
            PlayOneShot(clip, position);
        }
    }

    public void PlayOneShot(AudioClip clip, Vector3 position)
    {
        AudioSource audioSource = FindFreeOneShot();
        audioSource.transform.position = position;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void CrossFadeToMusic(AudioClip music, float crossFadeTime = 0.0f, int channel = 0, bool resetTime = true)
    {
        GetVars().CrossFadeMusic(music, crossFadeTime, channel, resetTime);
    }

    public AudioClip GetCurrentMusicClip(int channel = 0)
    {        
        return GetVars()?.musicAudioSources[channel]?.clip;
    }

    private AudioSource FindFreeOneShot()
    {
        List<AudioSource> oneShots = GetVars().oneShotAudioSources;
        
        for (int i = 0; i < oneShots.Count; i++)
        {
            int oneShotIndex = (lastOneShotIndex + 1 + i) % oneShots.Count;
            AudioSource oneShotSource = oneShots[oneShotIndex];

            if (!oneShotSource.isPlaying)
            {
                lastOneShotIndex = oneShotIndex;
                return oneShotSource;
            }
        }

        return null;
    }
}

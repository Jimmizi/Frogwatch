using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
struct AudioEventClip
{
    [SerializeField]
    public AudioEvent audioEvent;

    [SerializeField]
    public AudioClip clip;
}

public class AudioSystemVars : ServiceVars
{
    [Header("One Shots")]
    
    [SerializeField]
    private AudioSource _oneShotPrefab = null;

    [SerializeField]
    private int _maxNumOneShots = 10;

    public List<AudioSource> oneShotAudioSources = new List<AudioSource>();

    [Space(10), Header("Loops")]

    [SerializeField]
    private AudioSource _loopPrefab = null;

    [SerializeField]
    private int _maxNumLoopChannels = 2;

    public List<AudioSource> loopAudioSources = new List<AudioSource>();

    [Space(10), Header("Music")]

    [SerializeField]
    private AudioSource _musicSourcePrefab = null;

    [SerializeField]
    private int _maxNumMusicChannels = 2;

    public List<AudioSource> musicAudioSources = new List<AudioSource>();

    private List<AudioSource> _musicCrossAudioSources = new List<AudioSource>();

    [Space(10), Header("Events")]
    [SerializeField]
    List<AudioEventClip> eventClips;


    private void Awake()
    {
        for (int i = 0; i < _maxNumOneShots; i++)
        {
            AudioSource newOneShot = Instantiate(_oneShotPrefab);
            oneShotAudioSources.Add(newOneShot);
        }

        for (int i = 0; i < _maxNumLoopChannels; i++)
        {
            AudioSource newLoop = Instantiate(_loopPrefab);
            loopAudioSources.Add(newLoop);
        }

        for (int i = 0; i < _maxNumMusicChannels; i++)
        {
            AudioSource newMusic = Instantiate(_musicSourcePrefab);
            musicAudioSources.Add(newMusic);

            AudioSource newCrossMusic = Instantiate(_musicSourcePrefab);
            _musicCrossAudioSources.Add(newCrossMusic);
        }
    }

    public void CrossFadeMusic(AudioClip music, float crossFadeTime = 0.0f, int channel = 0, bool resetTime = true)
    {        
        if (channel < 0 || channel >= _maxNumMusicChannels)
        {
            return;
        }

        if (music == null)
        {
            musicAudioSources[channel].Stop();
        }
        else
        {
            StartCoroutine(DoCrossFadeChannel(music, channel, crossFadeTime, resetTime));
        }
    }

    public AudioClip FindAudioEventClip(AudioEvent audioEvent)
    {
        if (eventClips != null)
        {
            for (int i = 0; i < eventClips.Count; i++)
            {
                if (eventClips[i].audioEvent == audioEvent)
                {
                    return eventClips[i].clip;
                }
            }
        }

        return null;
    }

    IEnumerator DoCrossFadeChannel(AudioClip music, int channel, float crossFadeTime, bool resetTime)
    {
        // Setup cross-fade track in sync with main track volume 0
        _musicCrossAudioSources[channel].clip = music;
        _musicCrossAudioSources[channel].time = resetTime ? 0 : musicAudioSources[0].time;
        _musicCrossAudioSources[channel].volume = 0;
        _musicCrossAudioSources[channel].Play();

        float fadeTimer = crossFadeTime;
        while (true)
        {
            fadeTimer -= Time.deltaTime;
            if (fadeTimer <= 0) { break; }

            float fadeRate = fadeTimer / crossFadeTime;
            musicAudioSources[channel].volume = fadeRate;
            _musicCrossAudioSources[channel].volume = (1 - fadeRate);
            yield return null;
        }

        // Ensure final volumes and stop main track
        _musicCrossAudioSources[channel].volume = 1.0f;
        musicAudioSources[channel].volume = 0.0f;
        musicAudioSources[channel].Stop();

        // Swap the active music and the cross-faded music sources
        var temp = _musicCrossAudioSources[channel];
        _musicCrossAudioSources[channel] = musicAudioSources[channel];
        musicAudioSources[channel] = temp;
    }
}

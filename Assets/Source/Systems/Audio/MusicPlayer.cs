using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    enum TransitionType
    {
        Cancel,
        CrossFade,
        FadeOutIn,
    }

    [SerializeField]
    private AudioClip _music;

    //[SerializeField] // Transition types is a lie, there only cross fade :D
    private TransitionType _transitionType = TransitionType.CrossFade;

    [SerializeField]
    private float _transitionTime = 2.0f;

    [SerializeField]
    private bool _autoPlay = true;

    [SerializeField]
    private float _autoPlayDelay = 0.0f;

    [SerializeField]
    private int _musicChannel = 0;

    void Start()
    {
        if (_autoPlay)
        {
            if (_autoPlayDelay <= 0)
            {
                Play();
            }
            else
            {
                Invoke("Play", _autoPlayDelay);
            }
        }
    }

    public void Play()
    {
        var AudioSys = Service.Get<AudioSystem>();
        switch (_transitionType)
        { 
            case TransitionType.Cancel:                
                break;
            case TransitionType.CrossFade:
                AudioSys.CrossFadeToMusic(_music, _transitionTime, _musicChannel);
                break;
            case TransitionType.FadeOutIn:
                break;

            default:
                throw new System.InvalidOperationException();
        }
    }
}

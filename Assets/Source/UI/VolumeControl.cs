using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    public Sprite speakerNormalIcon;
    public Sprite speakerMutedIcon;

    public Image speaker;
    Slider slider;
    
    void Start()
    {
        slider = GetComponentInChildren<Slider>();

        AudioListener.volume = 0.25f;

        slider.value = AudioListener.volume;

        UpdateSpeaker();
    }

    void Update()
    {
        if (slider.value != AudioListener.volume)
        {
            AudioListener.volume = slider.value;
            UpdateSpeaker();
        }
    }

    void UpdateSpeaker()
    {
        if (AudioListener.volume > 0)
        {
            speaker.sprite = speakerNormalIcon;
        }
        else
        {
            speaker.sprite = speakerMutedIcon;
        }
    }
}

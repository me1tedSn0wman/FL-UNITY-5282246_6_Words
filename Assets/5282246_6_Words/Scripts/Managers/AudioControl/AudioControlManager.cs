using System;
using UnityEngine;
using Utils;

public class AudioControlManager : Soliton<AudioControlManager>
{
    [SerializeField] private static float _soundVolume;
    public static float soundVolume
    {
        get { return _soundVolume; }
        set {
            _soundVolume = Mathf.Clamp01(value);
            OnSoundVolumeChanged(_soundVolume);
        }
    }
    public static event Action<float> OnSoundVolumeChanged;


    [SerializeField] private static float _musicVolume;
    public static float musicVolume
    {
        get { return _musicVolume; }
        set { 
            _musicVolume = Mathf.Clamp01(value);
            OnMusicVolumeChanged(_musicVolume);
        }
    }
    public static event Action<float> OnMusicVolumeChanged;

    public void SetSoundVolumeWithoutNotification(float value) {
        _soundVolume = value;
    }

    public void SetMusicVolumeWithoutNotification(float value)
    {
        _musicVolume = value;
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : WindowUI
{
    [Header("Sliders")]
    [SerializeField] private Slider slider_SoundVolume;
    [SerializeField] private Slider slider_MusicVolume;

    [Header("Textes")]
    [SerializeField] private TextMeshProUGUI text_SoundVolume;
    [SerializeField] private TextMeshProUGUI text_MusicVolume;

    public new event Action OnPlayAudioUI;

    public override void Awake()
    {
        base.Awake();

        slider_SoundVolume.onValueChanged.AddListener((value) =>
        {
            AudioControlManager.soundVolume = value;
            text_SoundVolume.text = ((int)(value * 100)).ToString();
            PlayAudioUI();
        });
        slider_MusicVolume.onValueChanged.AddListener((value) => {
            AudioControlManager.musicVolume = value;
            text_MusicVolume.text = ((int)(value * 100)).ToString();
            PlayAudioUI();
        });

        UpdateInitValues();
    }

    public override void PlayAudioUI()
    {
        OnPlayAudioUI();
    }


    public void UpdateInitValues() {

        float tSoundVol = AudioControlManager.musicVolume;
        float tMusicVol = AudioControlManager.soundVolume;

        slider_SoundVolume.SetValueWithoutNotify(tSoundVol);
        slider_MusicVolume.SetValueWithoutNotify(tMusicVol);

        text_SoundVolume.text = ((int)(tSoundVol * 100)).ToString();
        text_MusicVolume.text = ((int)(tMusicVol * 100)).ToString();

    }
}

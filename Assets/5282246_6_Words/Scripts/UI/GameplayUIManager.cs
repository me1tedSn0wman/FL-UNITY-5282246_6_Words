using Utils;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIManager : Singleton<GameplayUIManager>
{
    [Header("Objects")]
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private WindowUI infoUI;

    [Header("Buttons")]
    [SerializeField] private Button button_Settings;
    [SerializeField] private Button button_Info;
    [SerializeField] private Button button_MainMenu;
    [SerializeField] private Button button_HelpButton;

    [SerializeField] private Button button_Check;

    [Header("Canvases")]

    [SerializeField] private GameObject canvasGO_GameOverGO;

    [Header("UI Audio Clip")]

    [SerializeField] private AudioClip audioClipUI;
    [SerializeField] private AudioControl audioControl;

    public override void Awake()
    {
        base.Awake();

        infoUI.SetActive(false);
        settingsUI.SetActive(false);

        settingsUI.OnPlayAudioUI += PlayAudioUI;
        infoUI.OnPlayAudioUI += PlayAudioUI;

        button_Settings.onClick.AddListener(() =>
        {
            PlayAudioUI();
            settingsUI.SetActive(true);
        });
        button_Info.onClick.AddListener(() =>
        {
            PlayAudioUI();
            infoUI.SetActive(true);
        });
        button_MainMenu.onClick.AddListener(() =>
        {
            PlayAudioUI();
            GameManager.LoadMainMenuScene();
        });
        button_HelpButton.onClick.AddListener(() => {
            PlayAudioUI();
            GameplayManager.Instance.ShowRandomWord();
        });

        button_Check.onClick.AddListener(() =>
        {
            GameplayManager.Instance.CheckWord();
        });
    }

    private void PlayAudioUI()
    {
        audioControl.PlayOneShot(audioClipUI);
    }

    public void OnDestroy()
    {
        settingsUI.OnPlayAudioUI -= PlayAudioUI;
        infoUI.OnPlayAudioUI -= PlayAudioUI;
    }


}
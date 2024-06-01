using System.Collections.Generic;
using Utils;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : Singleton<MainMenuUIManager>
{
    [SerializeField] private GameObject levelIconPrefab;
    [SerializeField] private Transform canvasForLevelIcons;
    [SerializeField] private Dictionary<int, LevelIcon> dictOfLevelIcons;

    [SerializeField] private GameObject gridViewportGO;
    [SerializeField] private GameObject contentGridLevelsGO;
    [SerializeField] private int numOfColumns;
    [SerializeField] private float paddingSize;
    [SerializeField] private float levelIconAspectRatio;

    private float levelIconWidth;
    private float levelIconHeight;


    [SerializeField] private GameObject canvas_InputWordGO;

    [Header("Objects")]
    [SerializeField] private SettingsUI settingsUI;
    [SerializeField] private WindowUI infoUI;
    [SerializeField] private InputWordWindowUI inputWindowUI;

    [Header("Buttons")]
    [SerializeField] private Button button_Settings;
    [SerializeField] private Button button_Info;
    [SerializeField] private Button button_InputWord;
    [SerializeField] private Button button_EndlessGame;

    [Header("GO")]
    [SerializeField] private float rotatingSpeed = 3f;
    [SerializeField] private bool isLoading = true;
    [SerializeField] private GameObject loadingScreenGO;
    [SerializeField] private GameObject loadingImageGO;

    [Header("UI Audio Clip")]

    [SerializeField] private AudioClip audioClipUI;
    [SerializeField] private AudioControl audioControl;

    public override void Awake() {
        base.Awake();

        infoUI.SetActive(false);
        settingsUI.SetActive(false);
        inputWindowUI.SetActive(false);

        isLoading = GameManager.Instance.isLoading;
        loadingScreenGO.SetActive(isLoading);

        settingsUI.OnPlayAudioUI += PlayAudioUI;
        infoUI.OnPlayAudioUI += PlayAudioUI;
        inputWindowUI.OnPlayAudioUI += PlayAudioUI;
        GameManager.OnScreenSizeChanged += ResizeElements;
        /*
         buttons
         */

        canvas_InputWordGO.SetActive(!GameManager.IS_MOBILE);

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
        button_InputWord.onClick.AddListener(() =>
        {
            PlayAudioUI();
            inputWindowUI.SetActive(true);
        });
        button_EndlessGame.onClick.AddListener(() =>
        {
            PlayAudioUI();
            GameManager.StartGameWithWord("");
        });

        Init();

        CreateLevelGrid();
    }

    public static void END_LOADING() {
        Instance.isLoading = false;
        Instance.loadingScreenGO.SetActive(false);
    }

    private void Update()
    {
        if (isLoading == true) {
            loadingImageGO.transform.Rotate(new Vector3(0, 0, rotatingSpeed), Space.Self);
        }
    }

    private void Init() { 
    
    }

    private void PlayAudioUI() {
        audioControl.PlayOneShot(audioClipUI);
    }


    public void CalculateLevelIconSize() {
        RectTransform rectTrans = gridViewportGO.GetComponent<RectTransform>();

        if (numOfColumns > 0)
        {
            levelIconWidth = (rectTrans.rect.width - (numOfColumns  * paddingSize)) / numOfColumns;
            levelIconHeight = levelIconWidth / levelIconAspectRatio;
        }
    }

    public void CreateLevelGrid() {
        Dictionary<int, Level> dictOfLevels = new Dictionary<int, Level>(GameManager.Instance.dictOfLevels);
        dictOfLevelIcons = new Dictionary<int, LevelIcon>();
        CalculateLevelIconSize();

        int i = 0;

        foreach (KeyValuePair<int, Level> kvp in dictOfLevels) {
            GameObject newLevelIconGO = Instantiate(levelIconPrefab, canvasForLevelIcons);
            LevelIcon newLevelIcon = newLevelIconGO.GetComponent<LevelIcon>();
            newLevelIcon.SetLevelIcon(kvp.Key, levelIconWidth, levelIconHeight);
            dictOfLevelIcons.Add(kvp.Key, newLevelIcon);

            newLevelIconGO.transform.localPosition = new Vector3(
                (i % numOfColumns) * (levelIconWidth + paddingSize),
                -(i / numOfColumns) * (levelIconHeight + paddingSize),
                0
                );
            i++;
        }
        ResizeContentGridLevel(((i-1) / numOfColumns) * (levelIconHeight + paddingSize));
        GameManager.Instance.UpdateAllLevelsInMainManager();
    }

    public void ResizeContentGridLevel(float height)
    {
        RectTransform rect = contentGridLevelsGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
    }

    public void UpdateLevelInfo(int num, int maxWords, int openWordsCount) {
        if (dictOfLevelIcons.Count >= num)
        {
            dictOfLevelIcons[num].UpdateLevelInfo(maxWords, openWordsCount);
        }
    }

    public void ResizeElements(Vector2Int newScreenSize) {
        CalculateLevelIconSize();

        foreach (KeyValuePair<int, LevelIcon> kvp in dictOfLevelIcons) {
            kvp.Value.SetSize(levelIconWidth, levelIconHeight);

            kvp.Value.transform.localPosition = new Vector3(
                ((kvp.Key-1) % numOfColumns) * (levelIconWidth + paddingSize),
                -((kvp.Key - 1) / numOfColumns) * (levelIconHeight + paddingSize),
                0
                );
        }
        ResizeContentGridLevel((dictOfLevelIcons.Count / numOfColumns) * (levelIconHeight + paddingSize));
    }

    public void OnDestroy()
    {
        settingsUI.OnPlayAudioUI -= PlayAudioUI;
        infoUI.OnPlayAudioUI -= PlayAudioUI;
        inputWindowUI.OnPlayAudioUI -= PlayAudioUI;
        GameManager.OnScreenSizeChanged -= ResizeElements;
    }
}

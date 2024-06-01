using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;
using YG;

public enum GameState { 
    PreGame,
    ParseWords,
    CountLevelWords,
    MainMenu, 
    Gameplay,
    GameOver,
}

public class GameManager : Soliton<GameManager>
{
    public static bool IS_MOBILE; 
    public static int MAX_WORDS_LEVEL_COUNT = 120;

    [SerializeField] private bool simulateMobile= false;

    private GameState _gameState;

    public GameState gameState
    {
        get { return _gameState; }
        set { _gameState = value; }
    }

    [Header("WordList")]
    public HashSet<string> LIST_OF_WORDS;
    public List<string> LIST_OF_LONG_WORDS;

    [SerializeField] public TextAsset wordListText;
    public int numToParseBeforeYield= 10000;
    public int numToCountBeforeYield = 10;
    public static int NUM_TO_PARSE_BEFORE_YIELD;
    public int wordLengthMin = 3;
    public int wordLengthMax = 15;

    public int wordLengthForEndless = 10;

    public int currLine = 0;
    public int crtnCountLevel = 0;
    public int totalLines;
    public int wordCount;

    private string[] lines;

    public List<LevelDefin> listOfLevelDefin;
    public Dictionary<int, Level> dictOfLevels;

    public bool[,] listOfOpenWords = new bool[101, MAX_WORDS_LEVEL_COUNT];

    public int selectedLevel=-1;
    public string selectedWord;

    public bool isLoading = true;

    [Header("Screen Size")]
    public Vector2Int screenSize;

    public static event Action<Vector2Int> OnScreenSizeChanged;

    public override void Awake()
    {
        base.Awake();
        isLoading = true;


        IS_MOBILE = false
            || YandexGame.EnvironmentData.isMobile
            || YandexGame.EnvironmentData.isTablet
            || simulateMobile;

        gameState = GameState.PreGame;

        LIST_OF_WORDS = new HashSet<string>();
        NUM_TO_PARSE_BEFORE_YIELD = numToParseBeforeYield;


        for (int i = 0; i < listOfOpenWords.GetLength(0);i++) {
            for (int j = 0; j < listOfOpenWords.GetLength(1); j++) {
                listOfOpenWords[i, j] = false;
            }
        }

        screenSize = new Vector2Int(Screen.width, Screen.height);
        OnScreenSizeChanged += Foo;

        selectedWord = "";

        MakeDictionaryOfLevels();
        InitWordsParse();
        
        gameState = GameState.MainMenu;
    }

    public void Update()
    {
        CheckScreenSize();
    }

    public void SetLevel(int levelNum) {
        selectedLevel = levelNum;
        selectedWord = dictOfLevels[levelNum].word;
    }

    public void SetLevel(string word)
    {
        selectedLevel = -1;
        selectedWord = word;
    }

    #region SceneManagement
    public static void LoadMainMenuScene() {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
    }

    public static void LoadGameplayScene() {
        SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
    }

    public static void StartGameWithWord(string word) {
        Instance.SetLevel(word.ToUpper());
        LoadGameplayScene();
    }
    #endregion SceneManagement

    #region DictionaryOfLevels

    public void MakeDictionaryOfLevels() {
        dictOfLevels = new Dictionary<int, Level>();

        foreach (LevelDefin levelDefin in listOfLevelDefin) {
            Level newLevel = new Level();
            newLevel.num = levelDefin.num;
            newLevel.word = levelDefin.word.ToUpper();
            newLevel.maxWords = 0;

            dictOfLevels.Add(newLevel.num, newLevel);
        }
    }

    public void InitCountDictionaryOfLevels() {
        gameState = GameState.CountLevelWords;
        StartCoroutine(CountDictionaryOfLevels());
    }

    public IEnumerator CountDictionaryOfLevels() {
        foreach (KeyValuePair<int, Level> kvp in dictOfLevels) {
            int tMaxWords = GameplayWordLevel.CountSubWordsInWord(kvp.Value.word, LIST_OF_WORDS);
            kvp.Value.maxWords = tMaxWords;

            crtnCountLevel++;
            if (crtnCountLevel % numToCountBeforeYield == 0)
            {
                yield return null;
            }
            
        }
        UpdateAllLevelsInMainManager();
        EndLoading();
    }

    public void UpdateAllLevelsOpenWords()
    {
        foreach (KeyValuePair<int, Level> kvp in dictOfLevels) {
            for (int i = 0; i < listOfOpenWords.GetLength(1); i++) {
                kvp.Value.SetWordOpen(i, listOfOpenWords[kvp.Key,i]);
            }
        }
        UpdateAllLevelsInMainManager();
    }

    public void UpdateAllLevelsInMainManager()
    { 
        foreach (KeyValuePair<int, Level> kvp in dictOfLevels) {
            MainMenuUIManager.Instance.UpdateLevelInfo(kvp.Value.num, kvp.Value.maxWords, kvp.Value.openWordsCount);
        }
    }

    #endregion DictionaryOfLevels

    #region DictionaryOfWords

    public void InitWordsParse() {
        lines = wordListText.text.Split('\n');
        totalLines = lines.Length;

        gameState = GameState.ParseWords;
        StartCoroutine(ParseWordsLines());
    }

    public IEnumerator ParseWordsLines() {
        string word;
        for (currLine = 0; currLine < totalLines; currLine++) {
            word = lines[currLine];
            if (
                    word.Length >= wordLengthMin
                    && word.Length <= wordLengthMax
                    && !LIST_OF_WORDS.Contains(word)
                ) 
                {
                LIST_OF_WORDS.Add(word);

                if (word.Length == wordLengthForEndless) {
                    LIST_OF_LONG_WORDS.Add(word);
                }

            }

            if (currLine % numToParseBeforeYield == 0) {
                wordCount = LIST_OF_WORDS.Count;
                yield return null;
            }
        }
        wordCount = LIST_OF_WORDS.Count;

        InitCountDictionaryOfLevels();
    }



    #endregion DictionaryOfWords

    public void EndLoading()
    {
        isLoading = false;
        MainMenuUIManager.END_LOADING();
        YandexGame.GameReadyAPI();
    }

    void CheckScreenSize() {
        Vector2Int newScreenSize = new Vector2Int(Screen.width, Screen.height);
        if (screenSize != newScreenSize) {
            OnScreenSizeChanged(newScreenSize);
            screenSize = newScreenSize;
        }
    }

    private void Foo(Vector2Int newSize) {
    
    }

    #region Saves

    private void OnEnable() => YandexGame.GetDataEvent += GetLoad;
    private void OnDisable() => YandexGame.GetDataEvent -= GetLoad;

    public void Save() {
        YandexGame.savesData.openWords = listOfOpenWords.Clone() as bool[,];
        YandexGame.SaveProgress();

        /*
        string str1 = "";
        string str2 = "";
        for (int i = 0; i < MAX_WORDS_LEVEL_COUNT; i++)
        {
            str1 += listOfOpenWords[1, i] ? "1" : "0";
            str2 += YandexGame.savesData.openWords[1, i] ? "1" : "0";
        }
        Debug.Log(str1);
        Debug.Log(str2);
        */
    }

    public void GetLoad() {
        listOfOpenWords = YandexGame.savesData.openWords.Clone() as bool[,];
        UpdateAllLevelsOpenWords();

        /*
        string str1 = "";
        string str2 = "";
        for (int i = 0; i < MAX_WORDS_LEVEL_COUNT; i++)
        {
            str1 += listOfOpenWords[1, i] ? "1" : "0";
            str2 += YandexGame.savesData.openWords[1, i] ? "1" : "0";
        }
        Debug.Log(str1);
        Debug.Log(str2);
        */
    }



    public void UpdateWordOpenList(int levelNum, int wordNum) {
        dictOfLevels[levelNum].SetWordOpen(wordNum,true);
        listOfOpenWords[levelNum,wordNum] = true;
        Save();
    }
    #endregion Saves


}


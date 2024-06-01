using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;
using YG;

public enum GameplayState { 
    PreGame,
    LoadWord,
    Layout,
    CreatePieces,
    WaitInput,
    SomethingMoving,
    CheckWord,
}

public class GameplayManager : Singleton<GameplayManager>
{
    [SerializeField] private GameplayState _gameplayState;
    public GameplayState gameplayState {
        get { return _gameplayState; }
        set { _gameplayState = value; }
    }

    [Header("Prefabs")]
    public GameObject prefabPieceLetter;
    public GameObject prefabWordLine;

    [Header("Canvases")]
    [SerializeField] private GameObject viewPortWordLinesGO;
    [SerializeField] private GameObject wordLinesAnchor;

    [SerializeField] private GameObject contentWordLinesGO;

    [SerializeField] private GameObject canvas_SelectLettersGO;

    private Level crntLevel;

    [Header("Other Things")]

    [SerializeField] private float pieceSize = 1.5f;
    public int numWordLineRows = 8;
    public int minColumnSize = 3;
    public float columnBaseXOffset = 0.5f;

    public float rowPadding = 0.1f;
    public float piecePadding = 1f;

    public bool[] openWords;

    [SerializeField] private string selectedWord;
    [SerializeField] private int selectedLevel;

    public GameplayWordLevel crntWordLevel;
    public Dictionary<char, int> charDict;

    public List<string> listOfSubWords;
    public List<WordLine> listOfWordLines;

    public List<PieceLetter> listOfSelectLetters;
    public List<PieceLetter> listOfSelectedLetters;

    public override void Awake()
    {
        base.Awake();
        gameplayState = GameplayState.PreGame;
        selectedWord = "";

        gameplayState = GameplayState.LoadWord;

        CalculatePieceSize();

        GameManager.OnScreenSizeChanged += ResizeElements;

        string crntWord = GameManager.Instance.selectedWord;
        selectedLevel = GameManager.Instance.selectedLevel;

        if (selectedLevel != -1) 
        {
            crntLevel = GameManager.Instance.dictOfLevels[selectedLevel];
            crntWordLevel = MakeWordLevel(selectedLevel);
        }
        else if (!string.Equals(crntWord, ""))
        {
            crntLevel = null;
            crntWordLevel = MakeWordLevel(crntWord);
        }
        else 
        {
            crntLevel = null;
            crntWordLevel = MakeWordLevel();
        }
    }

    public void CalculatePieceSize() {
        RectTransform rectTrans = viewPortWordLinesGO.GetComponent<RectTransform>();

        pieceSize = (rectTrans.rect.height -20f) / (numWordLineRows+1);
    }


    public GameplayWordLevel MakeWordLevel(int levelNum = -1) {
        
        GameplayWordLevel level = new GameplayWordLevel();
        string word;
        if (levelNum == -1)
        {
            level.levelNum = -1;

            int ind = Random.Range(0, GameManager.Instance.LIST_OF_LONG_WORDS.Count);
            word = GameManager.Instance.LIST_OF_LONG_WORDS[ind];
            openWords = new bool[0];
        }
        else {
            level.levelNum = levelNum;

            word = crntLevel.word;
            openWords = crntLevel.getOpenWords();
        }

        level.word = word;
        level.charDict = GameplayWordLevel.MakeCharDict(word);

        StartCoroutine(FindSubWordsCoroutine(level));
        return level;
    }

    public GameplayWordLevel MakeWordLevel(string levelWord)
    {
        GameplayWordLevel level = new GameplayWordLevel();

        level.levelNum = -1;
        level.word = levelWord;
        level.charDict = GameplayWordLevel.MakeCharDict(levelWord);

        StartCoroutine(FindSubWordsCoroutine(level));
        return level;
    }

    public IEnumerator FindSubWordsCoroutine(GameplayWordLevel level) {
        level.subWords = new List<string>();

        int i = 0;
        HashSet<string> words = GameManager.Instance.LIST_OF_WORDS;

        foreach (string word in words) {

            if (GameplayWordLevel.CheckWordInLevel(word, level)) {
                level.subWords.Add(word);
            }

            i++;
            if (i % GameManager.NUM_TO_PARSE_BEFORE_YIELD ==0) {
                yield return null;
            }
        }

        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();

        SubWordsSearchComplete();
    }

    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws) {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }

    public void SubWordsSearchComplete() {
        Layout();
    }

    void Layout() {
        gameplayState = GameplayState.Layout;

        SpawnWordLines();


        listOfSelectLetters = new List<PieceLetter>();
        listOfSelectedLetters = new List<PieceLetter>();


        SpawnSelectLetters();
        ArrangeSelectedLetters();
        
        gameplayState = GameplayState.WaitInput;
    }

    public void SpawnWordLines() {
        listOfWordLines = new List<WordLine>();

        float columnWidth;
        float leftOffeset = 0;

        for (int i = 0; i < crntWordLevel.subWords.Count; i++)
        {

            string word = crntWordLevel.subWords[i];
            columnWidth = Mathf.Max(minColumnSize, word.Length);

            WordLine wordLine = SpawnNewWordLine(word, leftOffeset, i % numWordLineRows);

            listOfWordLines.Add(wordLine);

            wordLine.visible = false;

            if (crntLevel != null && openWords.Length> i && openWords[i]) {
                wordLine.visible = true;
            }

            if (i % numWordLineRows == numWordLineRows - 1)
            {
                leftOffeset += (columnWidth + columnBaseXOffset) * pieceSize;
            }
        }

        ResizeContentCanvasWorldLines(leftOffeset);
    }

    public WordLine SpawnNewWordLine(string word, float xOffset, int crntRow) {
        GameObject newWordLineGO = Instantiate(prefabWordLine, wordLinesAnchor.transform);
        WordLine newWordLine = newWordLineGO.GetComponent<WordLine>();
        Vector3 tPos = new Vector3(
            xOffset,
            -(crntRow + 0.5f + crntRow * rowPadding) * pieceSize,
            0
            );
        newWordLine.SetWordLine(prefabPieceLetter, word, pieceSize, tPos);

        return newWordLine;
    }

    public void SpawnSelectLetters() {

        for (int i = 0; i < crntWordLevel.word.Length; i++)
        {
            char c = crntWordLevel.word[i];

            GameObject newPieceLetterGO = Instantiate(prefabPieceLetter, canvas_SelectLettersGO.transform);
            PieceLetter newPieceLetter = newPieceLetterGO.GetComponent<PieceLetter>();

            Vector3 tPos = new Vector3(
                i * pieceSize,
                0,
                0
                );

            newPieceLetter.SetPieceLetter(c, PieceLetterState.SelectList, pieceSize, tPos);

            listOfSelectLetters.Add(newPieceLetter);
        }
    }

    public void ResizeContentCanvasWorldLines(float sizeX) {
        RectTransform rect = contentWordLinesGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(sizeX, rect.sizeDelta.y);
    }

    public void SelectLetter(PieceLetter tPieceLetter) {
        selectedWord += tPieceLetter.charForTextMesh;

        listOfSelectLetters.Remove(tPieceLetter);
        listOfSelectedLetters.Add(tPieceLetter);
        ArrangeSelectedLetters();
    }

    public void RemoveLetter(PieceLetter tPieceLetter) {

        listOfSelectLetters.Add(tPieceLetter);
        listOfSelectedLetters.Remove(tPieceLetter);

        selectedWord = "";
        for (int i = 0; i < listOfSelectedLetters.Count; i++) {
            selectedWord += listOfSelectedLetters[i].str;
        }
        ArrangeSelectedLetters();
    }

    public void CheckWord() {
        for (int i = 0; i < listOfWordLines.Count; i++) {
            if (listOfWordLines[i].found) {
                continue;
            }

            string subWord = listOfWordLines[i].word;
            if (string.Equals(subWord, selectedWord)) {
                SetWordLineFound(i);
            }
        }
        

        ClearSelectedLetters();
    }

    public void ClearSelectedLetters()
    {
        selectedWord = "";

        foreach (PieceLetter tPieceLetter in listOfSelectedLetters) {
            listOfSelectLetters.Add(tPieceLetter);
        }
        listOfSelectedLetters.Clear();
        ArrangeSelectedLetters();
    }

    public void AddWordToListOfOpenWords(int wordNum) {
        if (crntLevel != null)
        {
            GameManager.Instance.UpdateWordOpenList(crntLevel.num, wordNum);
//            Debug.Log(crntLevel.num + "___" + wordNum);
        }
        else {
//            Debug.Log("there is no level ___" + wordNum);
        }
    }

    void ArrangeSelectedLetters() {

        float halfWidthSelectLetters = ((float)listOfSelectLetters.Count) / 2f;
        float halfWidthSelectedLetters = ((float)listOfSelectedLetters.Count) / 2f;

        for (int i = 0; i < listOfSelectLetters.Count; i++) {
            Vector3 tPos =
                new Vector3(
                    (i - halfWidthSelectLetters) * pieceSize,
                    -(0.5f+rowPadding)*pieceSize,
                    0
                    );


            listOfSelectLetters[i].StartMovingToLocPos(tPos, PieceLetterState.SelectList);
        }

        for (int i = 0; i < listOfSelectedLetters.Count; i++)
        {
            Vector3 tPos =
                    new Vector3(
                    (i - halfWidthSelectedLetters) * pieceSize,
                    (0.5f + rowPadding) * pieceSize,
                    0
                    );
            listOfSelectedLetters[i].StartMovingToLocPos(tPos, PieceLetterState.SelectedList);
        }
    }

    public void ResizeElements(Vector2Int newSize) {
        CalculatePieceSize();

        float leftOffeset = 0;
        float columnWidth;

        for (int i = 0; i < listOfWordLines.Count; i++) {
            

            columnWidth = Mathf.Max(minColumnSize, listOfWordLines[i].word.Length);
            int crntRow = i % numWordLineRows;
            Vector3 tPos = new Vector3(
                 leftOffeset,
                -(crntRow + 0.5f + crntRow * rowPadding) * pieceSize,
                0
                );

            listOfWordLines[i].SetPieceSize(pieceSize);
            listOfWordLines[i].SetWordLinePos(tPos);

            if (i % numWordLineRows == numWordLineRows - 1)
            {
                leftOffeset += (columnWidth + columnBaseXOffset) * pieceSize;
            }
        }
        ResizeContentCanvasWorldLines(leftOffeset);
        for (int i = 0; i < listOfSelectLetters.Count; i++) {
            listOfSelectLetters[i].SetPieceSize(pieceSize);
        }
        for (int i = 0; i < listOfSelectedLetters.Count; i++)
        {
            listOfSelectedLetters[i].SetPieceSize(pieceSize);
        }
        ArrangeSelectedLetters();

    }

    public void ShowRandomWord() {
        List<int> listOfIndex = new List<int>();

        for (int i = 0; i < listOfWordLines.Count; i++)
        {
            if (!listOfWordLines[i].found)
            {
                listOfIndex.Add(i);
            }
        }

        if (listOfIndex.Count > 0)
        {
            int randInd = Random.Range(0, listOfIndex.Count);
            SetWordLineFound(listOfIndex[randInd]);
        }
    }

    public void SetWordLineFound(int wordLineIndex) {
        listOfWordLines[wordLineIndex].SetFound(true);
        AddWordToListOfOpenWords(wordLineIndex);
    }

    public void OnDestroy()
    {
        GameManager.OnScreenSizeChanged -= ResizeElements;
    }
}
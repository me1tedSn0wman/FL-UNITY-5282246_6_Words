using System.Collections.Generic;
using UnityEngine;

public class WordLine: MonoBehaviour
{
    public string word;
    public List<PieceLetter> letters = new List<PieceLetter>();

    public bool found = false;

    private RectTransform rectTransform;

    private float piecePadding = 1f;

    public bool visible
    {
        get {
            if (letters.Count == 0) return false;
            return letters[0].visible;
        }
        set {
            foreach (PieceLetter l in letters) {
                switch (value) {
                    case true:
                        l.pieceLetterState = PieceLetterState.WordVisible;
                        break;
                    case false:
                        l.pieceLetterState = PieceLetterState.WordHidden;
                        break;
                }
            }
        }
    }

    public Color color {
        get {
            if (letters.Count == 0) return Color.black;
            return letters[0].color;
        }
        set {
            foreach (PieceLetter l in letters) {
                l.color = value;
            }
        }
    }

    public void SetFound(bool value) {
        found = value;
        visible = value;
    }

    public void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetWordLine(GameObject prefabPieceLetter, string word, float pieceSize, Vector3 localPos) {

        this.word = word;

        SetWordLineSize(pieceSize, word.Length);
        SetWordLinePos(localPos);

        for (int j = 0; j < word.Length; j++) {
            char c = word[j];

            GameObject newPieceLetterGO = Instantiate<GameObject>(prefabPieceLetter, transform);

            PieceLetter newPieceLetter = newPieceLetterGO.GetComponent<PieceLetter>();

            Vector3 tPos = new Vector3(
                j * (pieceSize + piecePadding),
                0,
                0
                );

            newPieceLetter.SetPieceLetter(c, PieceLetterState.WordHidden, pieceSize, tPos);

            letters.Add(newPieceLetter);
        }
    } 

    public void SetWordLineSize(float pieceSize, int wordLength)
    {
        rectTransform.sizeDelta = new Vector2(pieceSize * wordLength, pieceSize);
    }

    public void SetPieceSize(float pieceSize) {
        for (int i = 0; i < letters.Count; i++) {
            
            Vector3 tPos = new Vector3(
                i * (pieceSize + piecePadding),
                0,
                0
                );

            letters[i].SetPieceSize(pieceSize);
            letters[i].SetPieceLocPos(tPos);

        }

        rectTransform.sizeDelta = new Vector2(pieceSize * word.Length, pieceSize);
    }

    public void SetWordLinePos(Vector3 localPos) {
        this.transform.localPosition = localPos;
    }
}

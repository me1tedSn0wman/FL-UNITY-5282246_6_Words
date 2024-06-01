using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utils;

public enum PieceLetterState { 
    SelectList,
    SelectedList,
    WordHidden,
    WordVisible,
    MovingToPos,
    MovingToLocPos,
}

public class PieceLetter : MonoBehaviour, IPointerClickHandler
{

    [SerializeField] private Color selectColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color visibleColor;
    [SerializeField] private Color hiddenColor;

    [SerializeField] private PieceLetterState _pieceLetterState;
    public PieceLetterState pieceLetterState {
        get { return _pieceLetterState; }
        set {
            switch (value) {
                case PieceLetterState.SelectList:
                    visible = true;
                    color = selectColor;
                    break;
                case PieceLetterState.SelectedList:
                    visible = true;
                    color = selectedColor;
                    break;
                case PieceLetterState.WordVisible:
                    visible = true;
                    color = visibleColor;
                    break;
                case PieceLetterState.WordHidden:
                    visible = false;
                    color = hiddenColor;
                    break;
                default:
                    break;
            }
            _pieceLetterState = value; }
    }
    private PieceLetterState nextPieceLetterState;

    private Image image;

    public float timeMoveDuration = 0.5f;
    private float timeMoving = -1;
    public GameObject textMeshGO;

    public TextMeshProUGUI textMesh;

    private List<Vector3> pts = null;

    private float pieceSize;

    private char _char;
    private Renderer rendGO;

    private RectTransform rectTransform;

    public char charForTextMesh{
        get { return _char; }
        set {
            _char = value;
            textMesh.text = _char.ToString();
        }
    }

    public Color color {
        get { return image.color; }
        set {
            image.color = value;
        }
    }

    public string str {
        get { return _char.ToString(); }
        set { _char = value[0]; }
    } 

    public bool visible {
        get { return textMeshGO.active; }
        set { 
            textMeshGO.SetActive(value);
        }
    }

    private Vector3 pos {
        set {
            pts = new List<Vector3>() { transform.position, value };
        }
    }
    public Vector3 posImmediate {
        set {
            transform.position = value;
        }
    }

    private Vector3 locPos
    {
        set
        {
            pts = new List<Vector3>() { transform.localPosition, value };
        }
    }

    private void Awake()
    {
        textMesh = textMeshGO.GetComponent<TextMeshProUGUI>();
        image = GetComponent<Image>();

        rectTransform = GetComponent<RectTransform>();

        visible = false;
    }

    public void OnPointerClick(PointerEventData pointerEventData) {
        switch (pieceLetterState)
        {
            case PieceLetterState.SelectList:
                GameplayManager.Instance.SelectLetter(this);
                break;
            case PieceLetterState.SelectedList:
                GameplayManager.Instance.RemoveLetter(this);
                break;
        }
    }

    public void OnMouseUpAsButton()
    {
        switch (pieceLetterState) {
            case PieceLetterState.SelectList:
                GameplayManager.Instance.SelectLetter(this);
                break;
            case PieceLetterState.SelectedList:
                GameplayManager.Instance.RemoveLetter(this);
                break;
        }
    }

    public void Update()
    {
        switch (pieceLetterState) {
            case PieceLetterState.MovingToPos:
                MovingToPos();
                break;
            case PieceLetterState.MovingToLocPos:
                MovingToLocPos();
                break;
            default:
                break;
        }
    }

    public void SetPieceLetter(char letter, PieceLetterState pieceLetterState, float pieceSize, Vector3 localPos) {
        charForTextMesh = letter;
        this.pieceSize = pieceSize;
        this.transform.localPosition = localPos;
        this.pieceLetterState = pieceLetterState;

        SetPieceSize(pieceSize);
    }

    public void SetPieceSize(float pieceSize) { 
        rectTransform.sizeDelta = new Vector2 (pieceSize, pieceSize);
    }

    public void SetPieceLocPos(Vector3 localPos) {
        transform.localPosition = localPos;
    }

    #region MovingToPos
    public void StartMovingToPos(Vector3 pos, PieceLetterState nextPieceLetterState) {
        this.pieceLetterState = PieceLetterState.MovingToPos;
        this.nextPieceLetterState = nextPieceLetterState;
        this.pos = pos;
        this.timeMoving = 0;
    }

    public void MovingToPos() {
        if (timeMoving == -1) return;
        timeMoving += Time.deltaTime;
        float rTimeMove = timeMoving / timeMoveDuration;
        rTimeMove = Mathf.Clamp01(rTimeMove);
        Vector3 tPos = Util.Bezier(rTimeMove, pts);
        transform.position = tPos;

        if (rTimeMove == 1) {
            EndMovingToPos();
        }
    }

    public void EndMovingToPos()
    {
        this.timeMoving = -1;
        pieceLetterState = nextPieceLetterState;
    }
    #endregion MovingToPos

    #region MovingToLocPos

    public void StartMovingToLocPos(Vector3 pos, PieceLetterState nextPieceLetterState)
    {
        this.pieceLetterState = PieceLetterState.MovingToLocPos;
        this.nextPieceLetterState = nextPieceLetterState;
        this.locPos = pos;
        this.timeMoving = 0;
    }

    public void MovingToLocPos()
    {
        if (timeMoving == -1) return;
        timeMoving += Time.deltaTime;
        float rTimeMove = timeMoving / timeMoveDuration;
        rTimeMove = Mathf.Clamp01(rTimeMove);
        Vector3 tPos = Util.Bezier(rTimeMove, pts);
        transform.localPosition = tPos;

        if (rTimeMove == 1)
        {
            EndMovingToLocPos();
        }
    }

    public void EndMovingToLocPos()
    {
        this.timeMoving = -1;
        pieceLetterState = nextPieceLetterState;
    }
    #endregion MovingToLocPos
}
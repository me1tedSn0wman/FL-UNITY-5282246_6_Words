using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelIcon : MonoBehaviour
{
    [SerializeField] private int levelNum;

    [Header("Image")]

    [SerializeField] private Image iconImage;

    [Header("Color")]

    [SerializeField] private Color baseColor;
    [SerializeField] private Color startedColor;
    [SerializeField] private Color completeColor;

    [Header("Textes")] 

    [SerializeField] private TextMeshProUGUI text_LevelNum;
    [SerializeField] private TextMeshProUGUI text_LevelScore;

    [Header("Button")]

    [SerializeField] private Button button_Level;



    private RectTransform rectTrans;

    public Color color
    {
        get { return iconImage.color; }
        set
        {
            iconImage.color = value;
        }
    }

    public void Awake()
    {
        rectTrans = GetComponent<RectTransform>();

        button_Level.onClick.AddListener(() =>
        {
            GameManager.Instance.SetLevel(levelNum);
            GameManager.LoadGameplayScene();
        });
        UpdateLevelIcon();
    }

    public void SetLevelIcon(int levelNum, float width, float height) {
        this.levelNum = levelNum;
        SetSize(width, height);
        UpdateLevelIcon();
    }

    public void UpdateLevelIcon() {
        UpdateLevelInfo(0, 0);
    }

    public void UpdateLevelInfo(int maxWords, int openWordsCount) {
        text_LevelNum.text = levelNum.ToString();
        text_LevelScore.text = openWordsCount.ToString() +"/" + maxWords.ToString();

        if (openWordsCount == maxWords && maxWords > 0)
        {
            color = completeColor;
        }
        else if (maxWords > 0 && openWordsCount > 0 && openWordsCount < maxWords)
        {
            color = startedColor;
        }
        else 
        {
            color = baseColor;
        }
    }

    public void SetSize(float width, float height) {
        rectTrans.sizeDelta = new Vector2(width, height);
    }
}

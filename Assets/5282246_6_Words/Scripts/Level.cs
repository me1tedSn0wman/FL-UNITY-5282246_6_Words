public class Level
{
    public int num;
    public string word;

    public int maxWords=0;
    private bool[] openWords = new bool[GameManager.MAX_WORDS_LEVEL_COUNT];

    public void SetWordOpen(int wordNum, bool value) {
        openWords[wordNum] = value;
    }

    public int openWordsCount { get {
            int count = 0;
            for (int i = 0; i < openWords.Length && i< maxWords; i++) {
                if (openWords[i]) count++;
            }
            return count;
        }
    }

    public bool[] getOpenWords() {
        return openWords;
    }
}

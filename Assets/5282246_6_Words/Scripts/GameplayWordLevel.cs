using System.Collections.Generic;

[System.Serializable]
public class GameplayWordLevel
{
    public int levelNum;
    public string word;

    public Dictionary<char, int> charDict;
    public List<string> subWords;

    static public Dictionary<char, int> MakeCharDict(string tWord) {
        Dictionary<char, int> dict = new Dictionary<char, int>();
        char c;

        for (int i = 0; i < tWord.Length; i++) {
            c = tWord[i];
            if (dict.ContainsKey(c))
            {
                dict[c]++;
            }
            else {
                dict.Add(c, 1);
            }
        }
        return dict;
    }

    public static bool CheckWordInLevel(string str, GameplayWordLevel level) {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < str.Length; i++) {
            char c = str[i];
            if (level.charDict.ContainsKey(c))
            {
                if (!counts.ContainsKey(c))
                {
                    counts.Add(c, 1);
                }
                else
                {
                    counts[c]++;
                }

                if (counts[c] > level.charDict[c])
                {
                    return false;
                }
            }
            else {
                return false;
            }
        }
        return true;
    }


    public static bool CheckSubWord(string subWord, Dictionary<char, int> wordCharDict) {
        Dictionary<char, int> counts = new Dictionary<char, int>();
        for (int i = 0; i < subWord.Length; i++) {
            char c = subWord[i];
            if (wordCharDict.ContainsKey(c))
            {
                if (!counts.ContainsKey(c))
                {
                    counts.Add(c, 1);
                }
                else
                {
                    counts[c]++;
                }

                if (counts[c] > wordCharDict[c])
                {
                    return false;
                }
            }
            else {
                return false;
            }
        }

        return true;
    }

    public static int CountSubWordsInWord(string word, HashSet<string> listOfWords) {
        Dictionary<char, int> dict = MakeCharDict(word);
        int count = 0;
        foreach (string tWord in listOfWords) {
            if (CheckSubWord(tWord, dict))
                count++;
        }

        return count;
    }
}

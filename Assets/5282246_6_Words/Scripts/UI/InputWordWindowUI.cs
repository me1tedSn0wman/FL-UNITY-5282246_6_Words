using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputWordWindowUI : WindowUI
{
    [Header("InputWordWindowUI")]
    [Header("Buttons")]
    
    [SerializeField] private Button button_StartGameWithWord;

    [Header("Input field")]
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private int maxWordLength = 15;

    [Header("Set dynamically")]
    [SerializeField] private string inputWord;

    public new event Action OnPlayAudioUI;

    public override void Awake() {
        base.Awake();
        inputField.onValueChanged.AddListener((value) =>
        {
            UpdateInputWord(value);
        });

        button_StartGameWithWord.onClick.AddListener(() =>
        {
            PlayAudioUI();
            StartGame();
        });
    }

    public void StartGame() { 
        GameManager.StartGameWithWord(inputWord);
    }

    public override void PlayAudioUI()
    {
        OnPlayAudioUI();
    }

    public void UpdateInputWord(string newWord) {
        inputWord = newWord;
        if (newWord.Length > maxWordLength) {
            inputWord = newWord.Substring(0, maxWordLength);
        }
        inputField.text = inputWord;
    }
}

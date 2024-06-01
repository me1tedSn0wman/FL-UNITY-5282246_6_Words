using System;
using UnityEngine;
using UnityEngine.UI;

public class WindowUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button button_CloseWindowCanvas;

    public event Action OnPlayAudioUI;

    public virtual void Awake()
    {
        button_CloseWindowCanvas.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            PlayAudioUI();
        });
    }

    public virtual void SetActive(bool value) {
        gameObject.SetActive(value);
    }

    public virtual void PlayAudioUI() {
        OnPlayAudioUI();
    }
}

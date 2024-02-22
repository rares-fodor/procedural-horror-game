using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreenUI : MonoBehaviour
{
    public static SplashScreenUI Singleton { get; private set; }

    [SerializeField] private TMP_Text splashScreenMessage;
    [SerializeField] private Button backButton;

    private void Awake()
    {
        Singleton = this;
        splashScreenMessage.text = string.Empty;
        backButton.onClick.AddListener(() =>
        {
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoinCreate);
        });
    }

    public void SetMessage(string message)
    {
        splashScreenMessage.text = message;
    }
}

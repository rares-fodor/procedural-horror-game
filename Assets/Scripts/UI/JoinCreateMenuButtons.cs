using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class JoinCreateMenuButtons : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;


    private void Awake()
    {
        createGameButton.onClick.AddListener(() =>
        {
            Debug.Log("Host started!");
            NetworkGameController.Singleton.StartHost();
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyMain);
        });

        joinGameButton.onClick.AddListener(() =>
        {
            Debug.Log("Client started!");
            NetworkGameController.Singleton.StartClient();
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoining);
        });

        backButton.onClick.AddListener(() =>
        {
        });
    }
}
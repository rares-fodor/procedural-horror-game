using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;

public class LobbyButtons : MonoBehaviour
{
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button changeNameButton;

    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;

    [SerializeField] private Button monsterButton;
    [SerializeField] private TMP_Text monsterButtonText;

    [SerializeField] private Button nameChangeButton;

    [SerializeField] private TMP_Text serverIPText;

    private enum MonsterButtonState
    {
        IsSurvivor,
        IsMonster,
    }

    private MonsterButtonState monsterState;
    [SerializeField] private bool allPlayersReady;
    [SerializeField] private bool localPlayerReady;

    private void Awake()
    {
        monsterButton.onClick.AddListener(() =>
        {
            NetworkGameController.Singleton.MonsterToggleServerRpc(NetworkManager.Singleton.LocalClientId);
        });
        readyButton.onClick.AddListener(() =>
        {
            // Maintain this exact order. Toggle boolean first, set button visuals, and finally notify server.
            if (allPlayersReady)
            {
                // Load game scene, go through netgamecontroller get player's data move it to the next scene
                // We need it to instantiate prefabs correctly
                NetworkGameController.Singleton.StartGame();
            }
            else
            {
                localPlayerReady = !localPlayerReady;
                SetReadyButton();
                NetworkGameController.Singleton.PlayerReadyToggleServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        });

        nameChangeButton.onClick.AddListener(() =>
        {
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyNameChange);
        });

        leaveButton.onClick.AddListener(() =>
        {
            NetworkGameController.Singleton.Shutdown();
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoinCreate);    
        });

        monsterButtonText.text = "Play Monster";
        monsterButton.interactable = true;

        readyButtonText.text = "Ready";
        readyButton.interactable = true;

        allPlayersReady = false;
        localPlayerReady = false;
        monsterState = MonsterButtonState.IsSurvivor;
    }

    private void Start()
    {
        NetworkGameController.Singleton.OnMonsterToggle.AddListener(NetworkGameController_OnMonsterToggle);
        NetworkGameController.Singleton.OnClientConnected.AddListener(NetworkGameController_OnClientConnected);
        NetworkGameController.Singleton.OnAllPlayersReadyToggle.AddListener(NetworkGameController_OnAllPlayersReadyToggle);
        NetworkGameController.Singleton.OnHostStarted.AddListener(NetworkGameController_OnHostStarted);
    }

    private void OnDestroy()
    {
        NetworkGameController.Singleton.OnMonsterToggle.RemoveListener(NetworkGameController_OnMonsterToggle);
        NetworkGameController.Singleton.OnClientConnected.RemoveListener(NetworkGameController_OnClientConnected);
        NetworkGameController.Singleton.OnAllPlayersReadyToggle.RemoveListener(NetworkGameController_OnAllPlayersReadyToggle);
    }

    private void NetworkGameController_OnMonsterToggle(ulong clientId)
    {
        Debug.Log("Button disabled, no monster for you");
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            MonsterButtonStateToggle();
        }
        else
        {
            monsterButton.interactable = !monsterButton.interactable;
        }
    }

    private void NetworkGameController_OnHostStarted()
    {
        SetConnectionMessage();
    }

    private void NetworkGameController_OnClientConnected()
    {
        // Treats joining a lobby after the monster has been taken.
        if (NetworkGameController.Singleton.monsterTaken.Value == true)
        {
            monsterButton.interactable = false;
        }
        SetConnectionMessage();
    }

    private void NetworkGameController_OnAllPlayersReadyToggle(ulong clientId)
    {
        // This will only be invoked with the server's clientId
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (allPlayersReady)
            {
                SetReadyButton();
            }
            else
            {
                readyButtonText.text = "Start";
            }
            allPlayersReady = !allPlayersReady;
        }
    }

    private void MonsterButtonStateToggle()
    {
        // If clientId had disconnected, this step is skipped and the rest of the players enable their buttons
        if (monsterState == MonsterButtonState.IsSurvivor)
        {
            // Button text should be the opposite of the state. Read as, button should show the next state upon clicking
            monsterButtonText.text = "Play Survivor";
            monsterState = MonsterButtonState.IsMonster;
        }
        else
        {
            monsterButtonText.text = "Play Monster";
            monsterState = MonsterButtonState.IsSurvivor;
        }
    }

    private void SetReadyButton()
    {
        if (localPlayerReady)
        {
            // If player is ready button should switch to "unready"
            readyButtonText.text = "Unready";
        }
        else
        {
            readyButtonText.text = "Ready";
        }
    }

    private void SetConnectionMessage()
    {
        var connectionData = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData;
        var address = connectionData.Address;
        var port = connectionData.Port;
        serverIPText.text = $"Game hosted at {address}:{port}";
    }
}

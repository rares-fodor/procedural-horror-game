using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class NetworkGameController : NetworkBehaviour
{
    public static NetworkGameController Singleton { get; private set; }

    // Player list display
    [SerializeField] private PlayerListEntry playerListPrefab;
    [SerializeField] private GameObject playerListContainer;

    public UnityEvent OnClientFailedToJoin;
    public UnityEvent OnClientConnected;
    public UnityEvent<ulong> OnAllPlayersReadyToggle;       // ONLY INVOKE WITH SERVER'S CLIENTID
    public UnityEvent<ulong> OnMonsterToggle;

    public NetworkVariable<bool> monsterTaken;

    [SerializeField] private NetworkList<PlayerListData> playerList;
    [SerializeField] private List<PlayerListEntry> playerEntries;

    private int playerReadyCount;

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private Transform monsterPrefab;

    private NetworkVariable<int> playersAlive = new NetworkVariable<int>();
    private NetworkVariable<int> gameProgress = new NetworkVariable<int>();

    private enum PlayerDataValueToggle
    {
        Monster,
        Ready,
    }

    private void Awake()
    {
        Singleton = this;
        OnClientFailedToJoin = new UnityEvent();
        OnClientConnected = new UnityEvent();
        OnAllPlayersReadyToggle = new UnityEvent<ulong>();
        OnMonsterToggle = new UnityEvent<ulong>();
        monsterTaken = new NetworkVariable<bool>();

        DontDestroyOnLoad(gameObject);

        playerList = new NetworkList<PlayerListData>();
        playerList.OnListChanged += PlayerList_OnListChanged;
    }

    public override void OnNetworkSpawn()
    {
        playersAlive.OnValueChanged += PlayerKilledCallback;
        gameProgress.OnValueChanged += GameProgressedCallback;
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (var clientId in NetworkManager.ConnectedClientsIds)
        {
            Transform playerTransform;
            if (GetPlayerListDataByClientId(clientId).Value.monster)
            {
                playerTransform = Instantiate(monsterPrefab);
            }
            else
            {
                playerTransform = Instantiate(playerPrefab);
            }

            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void PlayerList_OnListChanged(NetworkListEvent<PlayerListData> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<PlayerListData>.EventType.Add)
        {
            CreateAndAddPlayerListEntry(changeEvent.Value);
            return;
        }
        if (changeEvent.Type == NetworkListEvent<PlayerListData>.EventType.Remove)
        {

            foreach (var entry in playerEntries)
            {
                if (entry.clientId == changeEvent.Value.clientId)
                {
                    playerEntries.Remove(entry);
                    Destroy(entry.gameObject);
                    return;
                }
            }
            return;
        }
        if (changeEvent.Type == NetworkListEvent<PlayerListData>.EventType.Insert)
        {
            // Player was modified and reinserted at the same index, previous deletion shifts following entry instances down to fill the space
            // Refresh the entire list to respect the order.
            foreach (var entry in playerEntries)
            {
                Destroy(entry.gameObject);
            }
            playerEntries.Clear();

            // Rebuild instances
            foreach(var player in playerList)
            {
                CreateAndAddPlayerListEntry(player);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Debug.Log($"There are {playerList.Count} clients connected");
            foreach(var player in playerList) {
                Debug.Log($"ID: {player.clientId}, Monster: {player.monster}");
            }
        }
    }

    public void StartGame()
    {
        if (playerReadyCount == playerList.Count)
        {
            GameSceneController.Singleton.LoadGameScene();
        }
        else
        {
            Debug.LogError("Not all players were ready when start was requested");
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        
        NetworkManager.Singleton.OnClientConnectedCallback += host_NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += host_NetworkManager_OnClientDisconnectCallback;
        
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        monsterTaken.Value = false;
    }

    public void StartClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += client_NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += client_NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void host_NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        if (playerReadyCount > 0 && playerReadyCount == playerList.Count)
        {
            OnAllPlayersReadyToggle.Invoke(NetworkManager.Singleton.LocalClientId);
        }
        playerList.Add(new PlayerListData { clientId = clientId, monster = false, ready = false });
    }

    private void host_NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        // Remove playerData if found
        var data = GetPlayerListDataByClientId(clientId);
        if (data != null)
        {
            if (data.Value.monster)
            {
                MonsterToggleClientRpc(clientId);
            }
            playerList.Remove((PlayerListData) data);
            if (data.Value.ready)
            {
                playerReadyCount--;
            }
            else
            {
                if (playerReadyCount > 0 && playerReadyCount == playerList.Count)
                {
                    OnAllPlayersReadyToggle.Invoke(NetworkManager.Singleton.LocalClientId);
                }
            }
        }
    }

    private void client_NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"{playerList.Count}");
        foreach (var player in playerList)
        {
            CreateAndAddPlayerListEntry(player);
        }
        OnClientConnected.Invoke();
    }

    private void client_NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        Debug.LogWarning("Client failed to connect!");
        OnClientFailedToJoin.Invoke();
    }

    private void NetworkManager_ConnectionApprovalCallback(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= Consts.MAX_PLAYER_COUNT)
        {
            response.Reason = "Game is full";
            response.Approved = false;
            return;
        }
        if (SceneManager.GetActiveScene().name == GameSceneController.Singleton.gameSceneName)
        {
            response.Reason = "Game is in progress";
            response.Approved = false;
            return;
        }
        response.Approved = true;
    }

    public PlayerListData? GetPlayerListDataByClientId(ulong clientId)
    {
        foreach (var data in playerList)
        {
            if (data.clientId == clientId) return data;
        }
        return null;
    }

    private void CreateAndAddPlayerListEntry(PlayerListData data)
    {
        var entry = Instantiate(playerListPrefab, playerListContainer.transform);
        
        entry.clientId = data.clientId;
        entry.playerNameText.text = data.clientId.ToString();
        entry.monsterText.text = data.monster == true ? "Monster" : "Survivor";
        entry.readyText.text = data.ready == true ? "Ready" : "Not Ready";

        playerEntries.Add(entry);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MonsterToggleServerRpc(ulong clientId)
    {
        UpdatePlayerDataToggledValue(clientId, PlayerDataValueToggle.Monster);
        monsterTaken.Value = !monsterTaken.Value;
        MonsterToggleClientRpc(clientId);
    }

    [ClientRpc]
    private void MonsterToggleClientRpc(ulong clientId)
    {
        OnMonsterToggle.Invoke(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerReadyToggleServerRpc(ulong clientId)
    {
        UpdatePlayerDataToggledValue(clientId, PlayerDataValueToggle.Ready);
        if (GetPlayerListDataByClientId(clientId).Value.ready == true)
        {
            PlayerReady();
        }
        else
        {
            PlayerUnready();
        }
    }

    private void UpdatePlayerDataToggledValue(ulong clientId, PlayerDataValueToggle toggledValue)
    {
        var oldData = GetPlayerListDataByClientId(clientId);
        var index = playerList.IndexOf(oldData.Value);

        var modifiedPlayer = new PlayerListData();
        modifiedPlayer.clientId = clientId;
        modifiedPlayer.monster = toggledValue == PlayerDataValueToggle.Monster ? !oldData.Value.monster : oldData.Value.monster;
        modifiedPlayer.ready = toggledValue == PlayerDataValueToggle.Ready ? !oldData.Value.ready : oldData.Value.ready;

        playerList.Remove(oldData.Value);
        playerList.Insert(index, modifiedPlayer);
    }

    private void PlayerUnready()
    {
        if (playerReadyCount == NetworkManager.Singleton.ConnectedClientsIds.Count) {
            OnAllPlayersReadyToggle.Invoke(NetworkManager.Singleton.LocalClientId);
        }
        playerReadyCount--;
    }
    private void PlayerReady()
    {
        playerReadyCount++;
        if (playerReadyCount == NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            OnAllPlayersReadyToggle.Invoke(NetworkManager.Singleton.LocalClientId);
        }
    }
    public void NotifyPlayerKilled()
    {
        if (!IsServer) { return; }
        playersAlive.Value -= 1;
    }

    public void NotifyGameProgressed()
    {
        if (!IsServer) { return; }
        gameProgress.Value++;
    }

    private void PlayerKilledCallback(int prev, int current)
    {
        // TODO Update HUD to reflect players' status
        Debug.Log($"{current} players remaining");
        if (current == 0)
        {
            UIController.Singleton.gameOverScreen.GameOver("All players defeated! Game over!");
        }
    }
    private void GameProgressedCallback(int prev, int curr)
    {
        UIController.Singleton.progressCounterController.DisplayGameProgress(curr);
        if (curr == Consts.PILLAR_COUNT)
        {
            UIController.Singleton.gameOverScreen.GameOver("All pillars activated! Survivors win!");
        }
    }
}
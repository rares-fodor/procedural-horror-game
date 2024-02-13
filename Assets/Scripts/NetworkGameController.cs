using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEditor;

public class NetworkGameController : NetworkBehaviour
{
    public static NetworkGameController Singleton { get; private set; }

    // Player list display
    [SerializeField] private PlayerListEntry playerListPrefab;
    [SerializeField] private GameObject playerListContainer;

    public UnityEvent OnClientFailedToJoin;
    public UnityEvent OnMonsterTaken;

    [SerializeField] private NetworkList<PlayerListData> playerList;
    [SerializeField] private List<PlayerListEntry> playerEntries;

    private void Awake()
    {
        Singleton = this;
        OnClientFailedToJoin = new UnityEvent();
        OnMonsterTaken = new UnityEvent();
        
        DontDestroyOnLoad(gameObject);

        playerList = new NetworkList<PlayerListData>();
        playerList.OnListChanged += PlayerList_OnListChanged;
    }

    private void PlayerList_OnListChanged(NetworkListEvent<PlayerListData> changeEvent)
    {
        if (changeEvent.Type == NetworkListEvent<PlayerListData>.EventType.Add)
        {
            Debug.Log("Change Event: Add");

            CreateAndAddPlayerListEntry(changeEvent.Value);
            return;
        }
        if (changeEvent.Type == NetworkListEvent<PlayerListData>.EventType.Remove)
        {
            Debug.Log("Change Event: Remove");
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

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        
        NetworkManager.Singleton.OnClientConnectedCallback += host_NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += host_NetworkManager_OnClientDisconnectCallback;
        
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += client_NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += client_NetworkManager_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void host_NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerList.Add(new PlayerListData { clientId = clientId, monster = false });
    }

    private void host_NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        // Remove playerData if found
        var data = GetPlayerListDataByClientId(clientId);
        if (data != null)
        {
            playerList.Remove((PlayerListData) data);
        }
    }

    private void client_NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"{playerList.Count}");
        foreach (var player in playerList)
        {
            CreateAndAddPlayerListEntry(player);
        }
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

        playerEntries.Add(entry);
    }

    [ServerRpc(RequireOwnership = false)]
    public void MonsterRequestedServerRpc(ulong clientId)
    {
        Debug.Log($"ClientId: {clientId} wants to play monster!");

        foreach (var it in playerList)
        {
            if (it.monster == true)
            {
                return;
            }
        }
        var oldPlayerData = GetPlayerListDataByClientId(clientId);
        var index = playerList.IndexOf(oldPlayerData.Value);

        var modifiedPlayer = new PlayerListData { clientId = oldPlayerData.Value.clientId, monster = true };
        playerList.Remove(oldPlayerData.Value);
        playerList.Insert(index, modifiedPlayer);

        MonsterTakenClientRpc();
    }

    [ClientRpc]
    private void MonsterTakenClientRpc()
    {
        OnMonsterTaken.Invoke();
    }
}

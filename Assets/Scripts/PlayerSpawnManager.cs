using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    private static List<uint> PlayerPrefabHashes = new List<uint>() { 3298726090, 4146449171 };
    
    public static void SetClientPlayerPrefab(int index)
    {
        if (index > PlayerPrefabHashes.Count)
        {
            Debug.LogError($"Trying to assign player prefab index {index}, but only {PlayerPrefabHashes.Count} are available");
            return;
        }
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogError("Set this before connecting!");
            return;
        }
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(index);
    }

    public static void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        int prefabIndex = System.BitConverter.ToInt32(request.Payload);

        if (prefabIndex < 0 || prefabIndex > PlayerPrefabHashes.Count)
        {
            Debug.LogError($"Client provided prefab index {prefabIndex}, but only {PlayerPrefabHashes.Count} are available");
            return;
        }
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = PlayerPrefabHashes[prefabIndex];
        response.Pending = false;
    }
}

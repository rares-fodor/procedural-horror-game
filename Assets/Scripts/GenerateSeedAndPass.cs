using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GenerateSeedAndPass : NetworkBehaviour
{
    private int initialValue = 11111;
    private NetworkVariable<int> seed = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            Debug.Log("Generating \"seed\"");
            seed.Value = initialValue + 2;
        } else
        {
            Debug.Log("Seed value: " + seed.Value.ToString());
        }
    }
}

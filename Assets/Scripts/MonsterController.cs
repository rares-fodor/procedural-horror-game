using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class MonsterController : PlayableEntity
{
    private GameController gameController;
    [SerializeField] private bool spawned;

    private void Awake()
    {
        spawned = false;
        gameController = FindObjectOfType<GameController>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (!IsServer) { return; }

        if (other.gameObject.CompareTag("Player") && spawned)
        {
            Debug.Log($"[Server] Monster collided with a player at {transform.position}");
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.TakeDamage();
        }
    }

    public override Vector3 GetSpawnLocation()
    {
        // Get plane extents
        var extents = LevelGenerator.planeExtents;

        // Clamp spawn locations to the outer edge of the map
        var spawnPoint = Random.insideUnitCircle.normalized * Random.Range(0.85f, 0.95f);
        spawnPoint.x = extents.x * spawnPoint.x;
        spawnPoint.y = extents.z * spawnPoint.y;

        MonsterSpawnedAndMovedServerRpc();
        return new Vector3(spawnPoint.x, 0.5f, spawnPoint.y);
    }

    [ServerRpc]
    private void MonsterSpawnedAndMovedServerRpc()
    {
        spawned = true;
    }
    
}

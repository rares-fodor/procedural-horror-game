using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Unity.Netcode;

public class MonsterController : PlayableEntity
{
    private GameController gameController;
    [SerializeField] private bool spawned;

    private PlayerController target;

    private void Awake()
    {
        spawned = false;
        gameController = FindObjectOfType<GameController>();
    }

    private void Update()
    {
        HandleMovement();
        if (Input.GetKeyDown(KeyCode.Z))
        {
            target = FindFirstObjectByType<PlayerController>();
            var position = target.transform.position;
            transform.position = new Vector3(position.x - 10, 0.5f, position.z - 10);
        }
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (!IsServer) { return; }

        if (other.gameObject.CompareTag("Player") && spawned)
        {
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

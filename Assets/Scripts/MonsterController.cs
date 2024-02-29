using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class MonsterController : PlayableEntity
{
    [SerializeField] private bool spawned;

    private PlayerController target;

    [SerializeField] private GameObject indicator;

    private List<GameObject> indicatorInstances = new List<GameObject>(Consts.PILLAR_COUNT);

    private void Awake()
    {
        spawned = false;
    }

    public override void OnNetworkSpawn()
    {
        playerNameText.text = NetworkGameController.Singleton.GetPlayerListDataByClientId(OwnerClientId).Value.name.ToString();

        if (!IsOwner) { return; }
        base.OnNetworkSpawn();

        for (int i = 0; i < Consts.PILLAR_COUNT; i++)
        {
            var instance = Instantiate(indicator, Vector3.zero, Quaternion.identity);
            indicatorInstances.Add(instance);
        }
    }

    private void Update()
    {
        if (!IsLocalPlayer) { return; }

        HandleMovement();
        if (Input.GetKeyDown(KeyCode.Z))
        {
            target = FindFirstObjectByType<PlayerController>();
            var position = target.transform.position;
            transform.position = new Vector3(position.x - 10, 0.5f, position.z - 10);
        }

        UpdateIndicatorPositions();
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

    private void UpdateIndicatorPositions()
    {
        var targets = GameController.Singleton.GetActivePillarPositions();
        if (targets == null || targets.Count == 0) { return; }

        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            var indicator = indicatorInstances[i];

            Vector3 direction = target - transform.position;
            Vector3 pos = transform.position + direction.normalized * 1.3f;
            indicator.transform.LookAt(target);
            indicator.transform.position = pos;
        }

        for (int i = targets.Count; i < indicatorInstances.Count; i++)
        {
            indicatorInstances[i].SetActive(false);
            Destroy(indicatorInstances[i]);
            indicatorInstances.RemoveAt(i);
            Debug.Log($"Players activated a pillar, current remaining count is: {indicatorInstances.Count}");
        }
    }
}

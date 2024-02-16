using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerController : PlayableEntity
{
    [SerializeField] private GameObject indicator;
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>();

    // Hint stuff
    public int remainingHints = 0;
    private GameObject indicatorInstance;
    private bool indicatorVisible = false;
    private Vector3 hintTarget;

    private NetworkVariable<int> hitPoints = new NetworkVariable<int>();

    private void Awake()
    {
        indicatorInstance = Instantiate(indicator, new Vector3(0,0,0), Quaternion.identity);
        indicatorInstance.SetActive(false);
        hitPoints.OnValueChanged += OnDamageTaken;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("Moved player to spawnpoint");
        if (IsServer)
        {
            isAlive.Value = true;
            hitPoints.Value = Consts.PLAYER_MAX_HP;
        }
        if (!IsOwner) { return; }
        base.OnNetworkSpawn();
        GameController.LocalPlayer = gameObject;
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if (!isAlive.Value) { return; }

        HandleMovement();

        if (remainingHints > 0)
        {
            if (Input.GetKey(Consts.HINT_KEY) && !indicatorVisible)
            {
                EnableHint();
                remainingHints--;
            }
        }
        if (indicatorVisible)
        {
            UpdateHintPosition();
        }
    }

    private void EnableHint()
    {
        GameObject closest = GameController.GetClosestObject(transform.position);
        if (closest != null)
        {
            hintTarget = closest.transform.position;
            indicatorInstance.SetActive(true);
            indicatorVisible = true;
            StartCoroutine(ExpireHint());
        }
    }

    private void UpdateHintPosition()
    {
        Vector3 direction = hintTarget - transform.position;
        Vector3 pos = transform.position + direction.normalized * 2.0f;
        indicatorInstance.transform.position = pos;
    }

    private IEnumerator ExpireHint()
    {
        yield return new WaitForSeconds(2);
        if (indicatorVisible)
        {
            indicatorVisible = false;
            indicatorInstance.SetActive(false);
        }
    }

    public override Vector3 GetSpawnLocation()
    {
        float spawnX = Random.Range(-20, 20);
        float spawnZ = Random.Range(-20, 20);
        return new Vector3(spawnX, 0.5f, spawnZ);
    }

    public void TakeDamage()
    {
        hitPoints.Value--;

        if (!IsServer) { return; }
        if (hitPoints.Value == 0)
        {
            isAlive.Value = false;
            GameController.Singleton.NotifyPlayerKilled();
        }
    }

    private void OnDamageTaken(int prev, int current)
    {
        if (IsLocalPlayer)
        {
            UIController.Singleton.HPIndicatorController.HP = current;
        }
    }
}

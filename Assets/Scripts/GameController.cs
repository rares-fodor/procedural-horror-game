using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameController : NetworkBehaviour
{
    public static GameController Singleton { get; private set; }

    // Game
    [HideInInspector] public UnityEvent GameProgressedEvent = new UnityEvent();

    // UI
    [HideInInspector] public UnityEvent<string> PlayerInteractableTriggerToggle = new UnityEvent<string>();
    [HideInInspector] public UnityEvent<string, Dialogue> DialogueStarted = new UnityEvent<string, Dialogue>();
    [HideInInspector] public UnityEvent DialogueFinished = new UnityEvent();

    // NPC
    [HideInInspector] public UnityEvent PlayerSafeZoneToggle = new UnityEvent();
    [HideInInspector] public UnityEvent<GameObject> NPCLocationChangedEvent = new UnityEvent<GameObject>();

    // Players
    [HideInInspector] public  UnityEvent PlayerDied = new UnityEvent();

    private List<GameObject> pillars;
    public static GameObject LocalPlayer;

    private NetworkVariable<int> playersAlive = new NetworkVariable<int>();
    private NetworkVariable<int> gameProgress = new NetworkVariable<int>();


    public void Awake()
    {
        Singleton = this;
    }

    public override void OnNetworkSpawn()
    {
        playersAlive.OnValueChanged += PlayerKilledCallback;
        gameProgress.OnValueChanged += GameProgressedCallback;
        if (!IsServer) { return; }
        playersAlive.Value = NetworkManager.Singleton.ConnectedClientsIds.Count - 1;
    }

    public override void OnNetworkDespawn()
    {
        playersAlive.OnValueChanged -= PlayerKilledCallback;
        gameProgress.OnValueChanged -= GameProgressedCallback;
    }


    public void RemovePillar(GameObject reference)
    {
        pillars.Remove(reference);
    }

    public void SetPillarList(List<GameObject> pillars)
    {
        this.pillars = pillars;
    }

    public GameObject GetClosestObject(Vector3 position)
    {
        if (pillars == null || pillars.Count == 0)
        {
            var pillarObjects = FindObjectsOfType<PillarController>();
            pillars = pillarObjects.Select(p => p.gameObject).ToList();
        }

        GameObject closest = pillars[0];
        float leastDistance = Vector3.Distance(pillars[0].transform.position, position);

        for (int i = 1; i < pillars.Count; i++)
        {
            float currentDistance = Vector3.Distance(pillars[i].transform.position, position);
            if (currentDistance < leastDistance)
            {
                leastDistance = currentDistance;
                closest = pillars[i];
            }
        }

        return closest;
    }

    public void NotifyPlayerKilled()
    {
        if (!IsServer) { return; }
        playersAlive.Value -= 1;
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

    public void NotifyGameProgressed()
    {
        if (!IsServer) { return; }
        gameProgress.Value++;
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

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
    [HideInInspector] public static UnityEvent GameProgressedEvent = new UnityEvent();

    // Player
    [HideInInspector] public static UnityEvent PlayerDied = new UnityEvent();
    [HideInInspector] public static UnityEvent PlayerSafeZoneToggle = new UnityEvent();

    // UI
    [HideInInspector] public static UnityEvent<string> PlayerInteractableTriggerToggle = new UnityEvent<string>();
    [HideInInspector] public static UnityEvent<string, Dialogue> DialogueStarted = new UnityEvent<string, Dialogue>();
    [HideInInspector] public static UnityEvent DialogueFinished = new UnityEvent();

    // NPC
    [HideInInspector] public static UnityEvent<GameObject> NPCLocationChangedEvent = new UnityEvent<GameObject>();

    private static List<GameObject> pillars;
    public static GameObject LocalPlayer;

    private NetworkVariable<int> playersAlive = new NetworkVariable<int>();

    // Holds references for all canvas elements
    private UIController UIController;

    private int progress = 0;

    public void Awake()
    {
        Singleton = this;
        UIController = FindObjectOfType<UIController>();
        GameProgressedEvent.AddListener(OnGameProgressed);
    }

    public override void OnNetworkSpawn()
    {
        playersAlive.OnValueChanged += PlayerKilledCallback;
        if (!IsServer) { return; }
        playersAlive.Value = NetworkManager.Singleton.ConnectedClientsIds.Count - 1;
    }

    public override void OnNetworkDespawn()
    {
        playersAlive.OnValueChanged -= PlayerKilledCallback;
    }

    private void PlayerKilledCallback(int prev, int current)
    {
        // TODO Update HUD to reflect players' status
        Debug.Log($"{current} players remaining");
        if (current == 0)
        {
            UIController.gameOverScreen.GameOver("All players defeated! Game over!");
        }
    }

    public static void RemovePillar(GameObject reference)
    {
        pillars.Remove(reference);
    }

    public static void SetPillarList(List<GameObject> pillars)
    {
        GameController.pillars = pillars;
    }

    public static GameObject GetClosestObject(Vector3 position)
    {
        if (pillars != null && pillars.Count > 0)
        {
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
        return null;
    }

    public void NotifyPlayerKilled()
    {
        if (!IsServer) { return; }
        playersAlive.Value -= 1;
    }

    private void OnGameProgressed()
    {
        progress++;
        if (progress == Consts.PILLAR_COUNT)
        {
            UIController.Singleton.gameOverScreen.GameOver("Survivors activated all pillars, victory!");
        }
    }
}

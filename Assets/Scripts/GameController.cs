using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
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


    [SerializeField] private GameObject player;

    private static List<GameObject> pillars;


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
}

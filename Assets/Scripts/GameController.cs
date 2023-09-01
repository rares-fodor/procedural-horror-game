using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    [HideInInspector] public static UnityEvent GameProgressedEvent = new UnityEvent();
    [HideInInspector] public static UnityEvent PlayerDied = new UnityEvent();
    [HideInInspector] public static UnityEvent<string> PlayerInteractableTriggerToggle = new UnityEvent<string>();
    [HideInInspector] public static UnityEvent<string, Dialogue> DialogueStarted = new UnityEvent<string, Dialogue>();
    [HideInInspector] public static UnityEvent PlayerSafeZoneToggle = new UnityEvent();
    [HideInInspector] public static UnityEvent<List<GameObject>> StoneLocationChangedEvent = new UnityEvent<List<GameObject>>();
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour
{
    [HideInInspector] public static UnityEvent GameProgressedEvent = new UnityEvent();
    [HideInInspector] public static UnityEvent PlayerDied = new UnityEvent();
    [HideInInspector] public static UnityEvent PlayerTriggerToggle = new UnityEvent();
    [HideInInspector] public static UnityEvent<List<GameObject>> StoneLocationChangedEvent = new UnityEvent<List<GameObject>>();
}

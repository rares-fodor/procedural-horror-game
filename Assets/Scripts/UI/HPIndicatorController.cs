using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HPIndicatorController : MonoBehaviour
{
    [SerializeField] private TMP_Text HPCounterText; 
    [SerializeField] private GameObject HPCounterUI; 
    
    private int hp;
    
    public int HP
    {
        get { return hp; }
        set
        {
            hp = value;
            HPCounterText.text = "HP: " + new string('I', hp);
        }
    }

    private void Awake()
    {
        // Set max hit points
        HPCounterText.text = "HP: " + new string('I', Consts.PLAYER_MAX_HP);
    }

    private void Start()
    {
        // Disable for monster
        if (NetworkGameController.Singleton.GetPlayerListDataByClientId(NetworkManager.Singleton.LocalClientId).Value.monster)
        {
            Debug.Log("Disabling HP indicator for monster");
            HPCounterUI.SetActive(false);
        }
    }
}

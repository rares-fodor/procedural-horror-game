using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerListEntry : MonoBehaviour
{
    [SerializeField] public TMP_Text playerNameText;
    [SerializeField] public TMP_Text monsterText;
    public ulong clientId;
}

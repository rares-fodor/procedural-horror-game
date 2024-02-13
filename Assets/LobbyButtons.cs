using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyButtons : MonoBehaviour
{
    [SerializeField] private Button ReadyButton;
    [SerializeField] private Button MonsterButton;
    [SerializeField] private Button LeaveButton;
    [SerializeField] private Button ChangeNameButton;

    private void Awake()
    {
        MonsterButton.onClick.AddListener(() =>
        {
            NetworkGameController.Singleton.MonsterRequestedServerRpc(NetworkManager.Singleton.LocalClientId);
        });
    }

    private void Start()
    {
        NetworkGameController.Singleton.OnMonsterTaken.AddListener(() => {
            Debug.Log("Button disabled, no monster for you");
            MonsterButton.interactable = false;
        });
    }
}

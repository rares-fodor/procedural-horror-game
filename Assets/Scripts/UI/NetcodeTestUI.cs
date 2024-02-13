using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetcodeTestUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button monsterButton;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            Debug.Log("Host started");
            PlayerSpawnManager.SetClientPlayerPrefab(0);
            NetworkManager.Singleton.ConnectionApprovalCallback = PlayerSpawnManager.ConnectionApprovalCallback;
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        clientButton.onClick.AddListener(() =>
        {
            Debug.Log("Client started");
            PlayerSpawnManager.SetClientPlayerPrefab(0);
            NetworkManager.Singleton.StartClient();
            Hide();
        });
        monsterButton.onClick.AddListener(() =>
        {
            Debug.Log("Monster started");
            PlayerSpawnManager.SetClientPlayerPrefab(1);
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
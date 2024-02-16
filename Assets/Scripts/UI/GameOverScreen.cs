using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameOverScreen : NetworkBehaviour
{
    [SerializeField] private TMP_Text gameOverMessage;
    [SerializeField] private GameObject GameOverUI;

    private void Awake()
    {
        GameOverUI.SetActive(false);
    }

    public void GameOver(string message)
    {
        GameOverUI.SetActive(true);
        gameOverMessage.text = message;
        StartCoroutine(StopGame());
    }

    private IEnumerator StopGame()
    {
        yield return new WaitForSeconds(5f);
        if (IsServer) { 
            NetworkManager.Singleton.Shutdown();
        }
        Application.Quit();
    }

}

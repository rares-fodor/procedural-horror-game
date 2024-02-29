using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameOverScreenController : NetworkBehaviour
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

        if (IsServer)
        { 
            GameSceneController.Singleton.LoadLobbyScene();

            List<PillarController> pillars = new List<PillarController>(FindObjectsOfType<PillarController>(true));
            while (pillars.Count > 0)
            {
                var pillar = pillars[^1];
                pillar.GetComponent<NetworkObject>().Despawn();
                Destroy(pillar.gameObject);
                pillars.RemoveAt(pillars.Count - 1);
            }
        }
    }

}

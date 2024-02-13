using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
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
    }
}

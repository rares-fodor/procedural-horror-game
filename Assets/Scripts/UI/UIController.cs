using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Singleton { get; private set; }

    [SerializeField] public GameOverScreen gameOverScreen;
    [SerializeField] public HPIndicatorController HPIndicatorController;

    private void Awake()
    {
        Singleton = this;
    }

}

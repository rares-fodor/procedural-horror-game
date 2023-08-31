using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleUIElement : MonoBehaviour
{
    [SerializeField] private GameObject UIContainer;

    private bool active = false;

    private void Awake()
    {
        GameController.PlayerTriggerToggle.AddListener(OnPlayerToggleTrigger);
    }

    private void Update()
    {
        UIContainer.SetActive(active);
    }

    void OnPlayerToggleTrigger()
    {
        active = !active;
    }
}

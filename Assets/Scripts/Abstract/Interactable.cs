using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Interactable : NetworkBehaviour
{
    private bool active = false;
    protected string interactMessage;
    [SerializeField] protected List<GameObject> playersInTrigger = new List<GameObject>();


    protected Interactable(string interactMessage)
    {
        this.interactMessage = interactMessage;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTrigger.Add(other.gameObject);

            PlayerController otherController = other.GetComponent<PlayerController>();
            if (!otherController.IsLocalPlayer) { return; }
            
            active = true;
            GameController.Singleton.PlayerInteractableTriggerToggle.Invoke(interactMessage);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTrigger.Remove(other.gameObject);
            
            PlayerController otherController = other.GetComponent<PlayerController>();
            if (!otherController.IsLocalPlayer) { return; }

            // Disable message only if it is still active when exiting the trigger.
            if (active)
            {
                active = false;
                GameController.Singleton.PlayerInteractableTriggerToggle.Invoke(interactMessage);
            }
        }
    }
    
    /// <summary>
    /// Attempts to enable the interact message. Will invoke if the
    /// message is already enabled.
    /// </summary>
    protected void EnableInteractMessage()
    {
        if (!active)
        {
            active = true;
            GameController.Singleton.PlayerInteractableTriggerToggle.Invoke(interactMessage);
        }
    }

    /// <summary>
    /// Attempts to disable the interact message. Will not invoke if the
    /// message is already disabled.
    /// </summary>
    protected void DisableInteractMessage()
    {
        if (active)
        {
            active = false;
            GameController.Singleton.PlayerInteractableTriggerToggle.Invoke(Consts.EMPTY_STR);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected bool playerInTrigger;
    private bool active = false;
    protected string interactMessage;


    protected Interactable(string interactMessage)
    {
        this.interactMessage = interactMessage;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            active = true;
            GameController.PlayerInteractableTriggerToggle.Invoke(interactMessage);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;

            // Disable message only if it is still active when exiting the trigger.
            if (active)
            {
                active = false;
                GameController.PlayerInteractableTriggerToggle.Invoke(interactMessage);
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
            GameController.PlayerInteractableTriggerToggle.Invoke(interactMessage);
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
            GameController.PlayerInteractableTriggerToggle.Invoke(Consts.EMPTY_STR);
        }
    }
}

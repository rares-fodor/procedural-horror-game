using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] Dialogue dialogue;

    // Toggled by the trigger callbacks
    private bool playerInRange = false;

    private bool interacted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            GameController.PlayerInteractableTriggerToggle.Invoke(Consts.DIALOGUE_MESSAGE);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Only invoke if player hasn't interacted with the object
            if (!interacted)
            {
                playerInRange = false;
                GameController.PlayerInteractableTriggerToggle.Invoke(Consts.DIALOGUE_MESSAGE);
            }
        }
    }

    private void Update()
    {
        if (playerInRange)
        {
            FacePlayer();
            if (Input.GetKeyDown(Consts.INTERACT_KEY))
            {
                interacted = true;
                GameController.PlayerInteractableTriggerToggle.Invoke(Consts.EMPTY_STR);
                Debug.Log($"Dialogue started! {Consts.NPC_NAME} {dialogue}");
                GameController.DialogueStarted.Invoke(Consts.NPC_NAME, dialogue);
            }
        }
    }

    /// <summary>
    /// Look at the player.
    /// </summary>
    void FacePlayer()
    {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }

}

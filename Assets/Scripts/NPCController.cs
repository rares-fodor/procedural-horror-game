using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCController : Interactable
{
    [SerializeField] private GameObject player;
    [SerializeField] Dialogue dialogue;

    private bool dialogueFinished = true;


    public NPCController() : base(Consts.DIALOGUE_MESSAGE) {}


    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (!dialogueFinished)
        {
            // Cancel dialogue
            GameController.DialogueStarted.Invoke(Consts.NPC_NAME, dialogue);
            dialogueFinished = true;
        }
    }

    private void Awake()
    {
        GameController.DialogueFinished.AddListener(OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        dialogueFinished = true;
        EnableInteractMessage();
    }

    private void Update()
    {
        if (playerInTrigger)
        {
            //FacePlayer();
            if (Input.GetKeyDown(Consts.INTERACT_KEY) && dialogueFinished) 
                StartDialogue();
        }
    }
    
    /// <summary>
    /// Invokes the dialogue message toggle and disables the dialogueFinished flag
    /// and the interact message.
    /// </summary>
    void StartDialogue()
    {
        GameController.DialogueStarted.Invoke(Consts.NPC_NAME, dialogue);
        dialogueFinished = false;
        DisableInteractMessage();
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

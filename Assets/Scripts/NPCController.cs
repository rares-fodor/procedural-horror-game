using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCController : Interactable
{
    [SerializeField] Dialogue dialogue;

    private bool dialogueFinished = true;


    public NPCController() : base(Consts.DIALOGUE_MESSAGE) {}


    protected override void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) { return; }
        base.OnTriggerExit(other);

        if (!dialogueFinished && other.gameObject == GameController.LocalPlayer)
        {
            // Cancel dialogue
            GameController.Singleton.DialogueStarted.Invoke(Consts.NPC_NAME, dialogue);
            dialogueFinished = true;
        }
    }

    private void Awake()
    {
        GameController.Singleton.DialogueFinished.AddListener(OnDialogueFinished);
    }

    private void OnDialogueFinished()
    {
        dialogueFinished = true;
        EnableInteractMessage();
    }

    private void Update()
    {
        if (playersInTrigger.Count > 0 && playersInTrigger.Contains(GameController.LocalPlayer))
        {
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
        GameController.Singleton.DialogueStarted.Invoke(Consts.NPC_NAME, dialogue);
        dialogueFinished = false;
        DisableInteractMessage();
    }


}

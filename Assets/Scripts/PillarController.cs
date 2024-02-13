using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PillarController : Interactable
{
    [SerializeField] private float timeToCollect = 5f;

    // Count of all players actively contributing to colleciton
    // Set with ServerRpc should not be modified by clients
    [SerializeField] private int playersInteractingCountServer = 0;         

    // Time since interact button was pressed
    [SerializeField] private NetworkVariable<float> progressTimer = new NetworkVariable<float>();

    private bool interacting = false;
    private Material dissolveMaterial;
    private ProgressBar progressBar;

    public PillarController() : base(Consts.INTERACT_MESSAGE) { }

    private void Awake()
    {
        dissolveMaterial = gameObject.GetComponent<Renderer>().material;
        dissolveMaterial.SetFloat("_ClipThreshold", 0f);
        
        progressBar = FindObjectOfType<ProgressBar>();
        progressBar.maximum = timeToCollect;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) { return; }

        progressTimer.Value = 0f;
    }

    private void Update()
    {
        CollectPillar();
    }

    /// <summary>
    /// Collect the pillar and invoke the game progress event if interact button is 
    /// held down for at least timeToCollect seconds.
    /// </summary>
    private void CollectPillar()
    {
        var localPlayer = GameController.LocalPlayer;

        // Determines whether the local player is inside the trigger and interacting
        // Will notify the server if both conditions hold and when either of them no longer holds.
        if (playersInTrigger.Contains(localPlayer))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                interacting = true;
                NotifyInteractStartedServerRpc();
            }
            if (Input.GetKeyUp(KeyCode.E) && interacting)
            {
                interacting = false;
                NotifyInteractStoppedServerRpc();
            }
            progressBar.Progress = progressTimer.Value;
            progressBar.IsVisible = interacting;
        }
        else if (interacting && !playersInTrigger.Contains(localPlayer))
        {
            interacting = false;
            NotifyInteractStoppedServerRpc();
        }

        if (!IsServer) { return; }
        if (playersInteractingCountServer == 0)
        {
            progressTimer.Value = 0f;
            return;
        }

        // Increment progress (server side)
        float speedUpModifier = 1f;
        if (playersInteractingCountServer > 2)
        {
            speedUpModifier = 1f + 0.2f * playersInteractingCountServer;
        }
        progressTimer.Value += Time.deltaTime * speedUpModifier;

        if (progressTimer.Value >= timeToCollect) { CollectItemClientRpc(); }
    }


    [ServerRpc(RequireOwnership = false)]
    private void NotifyInteractStartedServerRpc()
    {
        playersInteractingCountServer += 1;
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyInteractStoppedServerRpc()
    {
        playersInteractingCountServer -= 1;
    }

    /// <summary>
    /// Modify the alpha clipping threshold over time
    /// </summary>
    private void AdvanceVanishEffect()
    {
        float clipThreshold = dissolveMaterial.GetFloat("_ClipThreshold");
        
        clipThreshold = Mathf.Lerp(0f, 0.7f, progressTimer.Value / timeToCollect);
        dissolveMaterial.SetFloat("_ClipThreshold", clipThreshold);
    }

    /// <summary>
    /// Invoke the game progress event and disable instance.
    /// </summary>
    [ClientRpc]
    private void CollectItemClientRpc()
    {
        // Remove pillar from global list
        GameController.RemovePillar(gameObject);
        gameObject.SetActive(false);

        // NOTE: This will hide progress bars of players working on a different pillar.
        // The bar should become visible again the very next frame so it shouldn't be an issue.
        progressBar.IsVisible = false;

        // Notify that a pillar has been collected and the game progressed.
        GameController.GameProgressedEvent.Invoke();
        DisableInteractMessage();
    }
}

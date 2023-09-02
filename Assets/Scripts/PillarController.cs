using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PillarController : Interactable
{

    [SerializeField] private float timeToCollect = 5f;

    private Material dissolveMaterial;

    // Time since interact button was pressed
    private float collectionTimer = 0f;


    public PillarController() : base(Consts.INTERACT_MESSAGE) { }
 

    private void Start()
    {
        dissolveMaterial = gameObject.GetComponent<Renderer>().material;
        dissolveMaterial.SetFloat("_ClipThreshold", 0f);
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
        if (playerInTrigger)
        {
            // Collect if player holds down the E key longer than the value of timeToCollect
            if (Input.GetKey(KeyCode.E))
            {
                collectionTimer += Time.deltaTime;
                if (collectionTimer >= timeToCollect)
                {
                    CollectItem();
                }
            }
            else
            {
                collectionTimer = 0f;
            }
            AdvanceVanishEffect();
        }
    }

    /// <summary>
    /// Modify the alpha clipping threshold over time
    /// </summary>
    private void AdvanceVanishEffect()
    {
        float clipThreshold = dissolveMaterial.GetFloat("_ClipThreshold");
        
        clipThreshold = Mathf.Lerp(0f, 0.7f, collectionTimer / timeToCollect);
        dissolveMaterial.SetFloat("_ClipThreshold", clipThreshold);
    }

    /// <summary>
    /// Invoke the game progress event and disable instance.
    /// </summary>
    private void CollectItem()
    {
        // Remove pillar from global list
        GameController.RemovePillar(gameObject);
        gameObject.SetActive(false);
        
        // Notify that a pillar has been collected and the game progressed.
        GameController.GameProgressedEvent.Invoke();

        DisableInteractMessage();
    }
}

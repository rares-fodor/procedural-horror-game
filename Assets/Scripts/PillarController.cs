using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PillarController : MonoBehaviour
{
    private Material dissolveMaterial;

    // Toggled by the trigger callbacks
    public bool playerInTrigger { get; private set; } = false ;

    // Time since interact button was pressed
    private float collectionTimer = 0f;

    [SerializeField] private float timeToCollect = 5f;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player found a stone!");
            playerInTrigger = true;
            GameController.PlayerTriggerToggle.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left trigger!");
            playerInTrigger = false;
            GameController.PlayerTriggerToggle.Invoke();
        }
    }

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
    /// Invoke the game progress event and disable current instance.
    /// </summary>
    private void CollectItem()
    {
        // Tell game controller that a stone was found
        GameController.GameProgressedEvent.Invoke();

        // Invoke player trigger event to prevent UI elements from remaining active
        GameController.PlayerTriggerToggle.Invoke();

        gameObject.SetActive(false);
    }
}

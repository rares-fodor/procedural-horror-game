using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PillarController : MonoBehaviour
{
    private Material dissolveMaterial;

    private bool isInTrigger = false;
    private float collectionTimer = 0f;

    [SerializeField] private float timeToCollect = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player found a stone!");
            isInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player left trigger!");
            isInTrigger = true;
        }
    }

    private void Start()
    {
        dissolveMaterial = gameObject.GetComponent<Renderer>().material;
        dissolveMaterial.SetFloat("_ClipThreshold", 0f);
    }

    private void Update()
    {
        if (isInTrigger)
        {
            // Collect if player holds down the E key longer than the value of timeToCollect
            if (Input.GetKey(KeyCode.E))
            {
                collectionTimer += Time.deltaTime;
                if (collectionTimer >= timeToCollect)
                {          
                    CollectItem();
                }
            } else
            {
                collectionTimer = 0f;
            }
            AdvanceVanishEffect();
        }
    }

    private void AdvanceVanishEffect()
    {
        float clipThreshold = dissolveMaterial.GetFloat("_ClipThreshold");
        
        clipThreshold = Mathf.Lerp(0f, 0.7f, collectionTimer / timeToCollect);
        dissolveMaterial.SetFloat("_ClipThreshold", clipThreshold);
    }

    private void CollectItem()
    {
        // Tell game controller that a stone was found
        GameController.GameProgressedEvent.Invoke();

        gameObject.SetActive(false);
    }
}
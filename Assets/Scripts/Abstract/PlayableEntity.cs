using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using TMPro;
using Unity.Collections;

public abstract class PlayableEntity : NetworkBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected Camera playerCamera;
    [SerializeField] protected GameObject cameraToggle;

    [SerializeField] protected TMP_Text playerNameText;


    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) { return; }
        cameraToggle.SetActive(true);
        transform.position = GetSpawnLocation();
    }

    public abstract Vector3 GetSpawnLocation();

    
    // The player moves either relative to the camera or to itself
    // depending on whether the player is freelooking (behind itself for example).
    protected void HandleMovement()
    {
        if (FreeLooking())
        {
            ApplyRotation();
            ApplyTranslation(playerCamera.transform);
        }
        else
        {
            ApplyTranslation(transform);
        }
    }

    private bool FreeLooking()
    {
        return Input.GetMouseButton(1);
    }

    private void ApplyRotation()
    {
        Quaternion rotation = playerCamera.transform.rotation;
        rotation.z = 0f;
        rotation.x = 0f;

        transform.rotation = rotation;
    }

    private void ApplyTranslation(Transform tr)
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = tr.forward;
        Vector3 right = tr.right;

        // Ignore y data and normalize to preserve movement speed
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;

        // Transform to local space
        Vector3 forwardRelative = v * forward;
        Vector3 rightRelative = h * right;

        Vector3 direction = (forwardRelative + rightRelative).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    protected void PlayerName_OnValueChanged(FixedString128Bytes prev, FixedString128Bytes current)
    {
        Debug.Log($"setting name to {current}");
        playerNameText.text = current.ToString();
    }
}

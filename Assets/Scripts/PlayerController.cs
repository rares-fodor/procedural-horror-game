using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMovement();
    }

    /// <summary>
    /// Updates the player transform to reflect its movement
    /// </summary>
    /// <remarks>
    /// The player moves either relative to the camera or to itself
    /// depending on whether the player is freelooking (behind itself for example).
    /// </remarks>
    private void HandleMovement()
    {
        if (FreeLooking())
        {
            ApplyRotation();
            ApplyTranslation(mainCamera.transform);
        }
        else
        {
            ApplyTranslation(transform);
        }
    }

    /// <summary>
    /// Detects if the user is freelooking.
    /// </summary>
    /// <remarks>
    /// If user is holding down LMB, the camera can rotate around the player
    /// without affecting its direction.
    /// </remarks>
    private bool FreeLooking()
    {
        return Input.GetMouseButton(1);
    }

    /// <summary>
    /// Applies the camera's rotation around the y axis to the player.
    /// </summary>
    private void ApplyRotation()
    {
        Quaternion rotation = mainCamera.transform.rotation;
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
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

    private bool FreeLooking()
    {
        return Input.GetMouseButton(1);
    }

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
    private void ApplyRotationToPoint()
    {
        // Rotation
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = mainCamera.transform.position.y - transform.position.y;

        // Raycast mouse position to ground
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Target separate layer for ground only (ignores other objects like trees and rocks)
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
        {
            Vector3 targetDirection = hit.point - transform.position;

            // Angle to target position
            float angle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;

            // Rotate only around the y-axis
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
}

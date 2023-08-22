using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Camera mainCamera;

    private float h;
    private float v;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
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

        // Translation
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        // Normalized movement vector
        Vector3 movement = new Vector3(h, 0, v).normalized;

        transform.position += movement * speed * Time.deltaTime;
    }
}

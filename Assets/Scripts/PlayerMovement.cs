using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    private float h;
    private float v;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Rotation
        Vector3 mousePosition = Input.mousePosition;

        // Force ScreenToWorld to return the the y coordinate of the player
        // instead of the distance from the camera plane
        mousePosition.z = mainCamera.transform.position.y - transform.position.y;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        transform.LookAt(mouseWorldPosition);

        // Translation
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        // Normalized movement vector
        Vector3 movement = new Vector3(h, 0, v).normalized;

        transform.position += movement * speed * Time.deltaTime;
    }
}

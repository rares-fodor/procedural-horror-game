using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;    
    }

    private void Update()
    {
        if (cameraTransform == null) {
            Debug.Log("No main camera found for UIFaceCamera script");
            return;
        }
        transform.LookAt(cameraTransform);
        transform.Rotate(0, 180, 0);
    }
}

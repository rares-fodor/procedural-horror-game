using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 30f; // Speed of rotation in degrees per second

    void Update()
    {
        // Calculate the rotation amount based on time
        float rotationAmount = rotationSpeed * Time.deltaTime;

        // Rotate the object around the y-axis
        transform.Rotate(Vector3.up, rotationAmount);
    }
}
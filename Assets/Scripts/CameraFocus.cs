using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] Camera cam;
    [Tooltip("Maximum distance increment from the player position\n Creates a \"box\" around the player that limits how far the focus can move away from the player")]
    [SerializeField] float threshold;
    [Tooltip("Where between the mouse position and the player should the camera focus.\n ex. 0.5 = halfway\nValues between 0 and 1")]
    [SerializeField] float focusFactor;

    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.transform.position.y;

        // Determine position of focus object
        Vector3 position = (cam.ScreenToWorldPoint(mousePosition) + player.transform.position) * focusFactor;

        // Limit focus object position by the threshold value
        position.x = Mathf.Clamp(position.x, player.transform.position.x - threshold, player.transform.position.x + threshold);
        position.z = Mathf.Clamp(position.z, player.transform.position.z - threshold, player.transform.position.z + threshold);

        transform.position = position;
    }
}

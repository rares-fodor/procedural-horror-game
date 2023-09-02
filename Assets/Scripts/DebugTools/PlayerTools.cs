using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTools : MonoBehaviour
{
    [SerializeField] private bool teleportToNextStone = false;

    private void Update()
    {
        if (teleportToNextStone)
        {
            TeleportToNextStone();
            teleportToNextStone = false;
        }
    }

    private void TeleportToNextStone()
    {
        GameObject closest = GameController.GetClosestObject(transform.position);
        if (closest != null)
        {
            Debug.Log("Teleporting player!");
            Vector3 stonePos = closest.transform.position;
            Vector3 target = new Vector3(stonePos.x + 4, 0.5f, stonePos.z + 4);
            gameObject.transform.position = target;
        }
    }

}

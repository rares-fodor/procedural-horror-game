using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTools : MonoBehaviour
{
    [SerializeField] private bool teleportToNextStone = false;

    private List<GameObject> stones;

    private void Awake()
    {
        GameController.StoneLocationChangedEvent.AddListener(OnStoneLocationsChanged);
    }

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
        GameObject closest = GetClosestStone();
        if (closest != null)
        {
            Debug.Log("Teleporting player!");
            Vector3 stonePos = closest.transform.position;
            Vector3 target = new Vector3(stonePos.x + 4, 0.5f, stonePos.z + 4);
            gameObject.transform.position = target;
        }
    }

    private void OnStoneLocationsChanged(List<GameObject> stones)
    {
        this.stones = stones;
    }

    private GameObject GetClosestStone()
    {
        if (stones != null && stones.Count > 0)
        {
            Vector3 target = gameObject.transform.position;
            GameObject closest = stones[0];
            float leastDistance = Vector3.Distance(stones[0].transform.position, target);

            for (int i = 1; i < stones.Count; i++)
            {
                float currentDistance = Vector3.Distance(stones[i].transform.position, target);
                if (currentDistance < leastDistance)
                {
                    leastDistance = currentDistance;
                    closest = stones[i];
                }
            }

            return closest;
        }
        return null;
    }

}

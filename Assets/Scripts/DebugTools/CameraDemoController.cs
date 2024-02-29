using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDemoController : MonoBehaviour
{
    [SerializeField] private List<GameObject> cameras = new List<GameObject>();

    int index = 0;

    private void Awake()
    {
        foreach(GameObject go in cameras)
        {
            go.SetActive(false);
        }
    }

    private void Start()
    {
        index = 0;
        cameras[index].SetActive(true);
    }

    private void Update()
    {
        // Cycle cameras
        if (Input.GetKeyDown(KeyCode.W))
        {
            cameras[index++].SetActive(false);
            if (index == cameras.Count)
            {
                index = 0;
            }
            cameras[index].SetActive(true);
            
        }
    }
}

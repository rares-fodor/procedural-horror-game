using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class BakeNavmesh : MonoBehaviour
{
    NavMeshSurface surface;

    void Awake()
    {
        surface = gameObject.GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
    }
}

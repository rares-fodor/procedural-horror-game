using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestBounds : MonoBehaviour
{

    [SerializeField] private GameObject testedObject;
    [SerializeField] private bool recompute;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (recompute)
        {
            Collider mr = testedObject.GetComponent<Collider>();
            Debug.Log(mr.bounds.extents.magnitude);
            recompute = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MouseInputHandler : MonoBehaviour
{
    void Start()
    {
        CinemachineCore.GetInputAxis = CameraAxisOverride;
    }
    public float CameraAxisOverride(string axisName)
    {
        // Only allow camera rotation when RMB or LMB is held down
        if (axisName == "Mouse X")
            return (Input.GetMouseButton(0) || Input.GetMouseButton(1)) ? UnityEngine.Input.GetAxis("Mouse X") : 0;
        else if (axisName == "Mouse Y")
            return (Input.GetMouseButton(0) || Input.GetMouseButton(1)) ? UnityEngine.Input.GetAxis("Mouse Y") : 0;
        
        return UnityEngine.Input.GetAxis(axisName);
    }
}

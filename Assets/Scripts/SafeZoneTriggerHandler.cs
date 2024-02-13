using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZoneTriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameController.PlayerSafeZoneToggle.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        GameController.PlayerSafeZoneToggle.Invoke();
    }
}

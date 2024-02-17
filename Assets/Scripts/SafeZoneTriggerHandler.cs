using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZoneTriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameController.Singleton.PlayerSafeZoneToggle.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        GameController.Singleton.PlayerSafeZoneToggle.Invoke();
    }
}

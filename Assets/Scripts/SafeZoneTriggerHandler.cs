using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZoneTriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameController.PlayerSafeZoneToggle.Invoke();
        Debug.Log("Player entered safe zone");
    }

    private void OnTriggerExit(Collider other)
    {
        GameController.PlayerSafeZoneToggle.Invoke();
        Debug.Log("Player left safe zone");
    }
}

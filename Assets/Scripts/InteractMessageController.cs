using TMPro;
using UnityEngine;



public class InteractMessageController : MonoBehaviour
{
    [SerializeField] private GameObject UIContainer;
    [SerializeField] private TMP_Text interactMessage;

    private bool active = false;

    private void Awake()
    {
        GameController.PlayerInteractableTriggerToggle.AddListener(OnPlayerToggleTrigger);
    }

    private void Update()
    {
        UIContainer.SetActive(active);
    }

    void OnPlayerToggleTrigger(string message)
    {
        active = !active;
        interactMessage.text = message;
    }
}

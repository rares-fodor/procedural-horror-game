using TMPro;
using UnityEngine;



public class InteractMessageController : MonoBehaviour
{
    [SerializeField] private GameObject UIContainer;
    [SerializeField] private TMP_Text interactMessage;

    [SerializeField] private bool visible = false;

    private void Awake()
    {
        GameController.PlayerInteractableTriggerToggle.AddListener(OnPlayerToggleTrigger);
    }

    private void Update()
    {
        UIContainer.SetActive(visible);
    }

    void OnPlayerToggleTrigger(string message)
    {
        visible = !visible;
        interactMessage.text = message;
    }
}

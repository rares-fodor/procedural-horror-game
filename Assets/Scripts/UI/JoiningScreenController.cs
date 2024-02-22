using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class JoiningScreenController : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private Button backButton;

    private string titleMessage;

    private void Awake()
    {
        titleMessage = title.text;
        gameObject.SetActive(false);
        backButton.onClick.AddListener(() =>
        {
            NetworkGameController.Singleton.Shutdown();
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoinCreate);
        });
    }

    private void Start()
    {
        NetworkGameController.Singleton.OnClientFailedToJoin.AddListener(NetworkGameController_OnClientFailedToJoin);
    }

    private void NetworkGameController_OnClientFailedToJoin()
    {
        StopAllCoroutines();
        title.text = NetworkManager.Singleton.DisconnectReason;
        if (title.text == "")
        {
            title.text = "Connection timed out";
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyMain);
        }
    }

    private void OnDestroy()
    {
        NetworkGameController.Singleton.OnClientFailedToJoin.RemoveListener(NetworkGameController_OnClientFailedToJoin);
    }

    private void OnEnable()
    {
        StartCoroutine(CycleDots());
    }

    private void OnDisable()
    {
        StopCoroutine(CycleDots());
    }

    private IEnumerator CycleDots()
    {
        int dots = 0;

        while (true)
        {
            title.text = $"{titleMessage}{new string('.', dots % 4)}";
            yield return new WaitForSeconds(1f);
            dots++;
        }
    }

}

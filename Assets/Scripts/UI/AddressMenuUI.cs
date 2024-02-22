using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Net;

public class AddressMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;

    [SerializeField] private TMP_Text diagnosticText;

    private IPAddress ipAddress;
    private bool validated = false;

    private UnityTransport netcodeTransport;

    private void Awake()
    {
        inputField.onEndEdit.AddListener(AddressInput_OnEndEdit);
        joinButton.onClick.AddListener(JoinButton_OnClick);
        backButton.onClick.AddListener(BackButton_OnClick);

        diagnosticText.text = string.Empty;
    }

    private void Start()
    {
        netcodeTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    private void AddressInput_OnEndEdit(string input)
    {
        // Fires when pressing either Enter buttons, Esc or when clicking out of the input field
        validated = IPAddress.TryParse(input, out ipAddress) && ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private void JoinButton_OnClick()
    {
        if (!validated)
        {
            diagnosticText.text = "Value is not valid, try again";
            return;
        }
        Debug.Log(ipAddress.ToString());
        netcodeTransport.SetConnectionData(ipAddress.ToString(), 7777);
        NetworkGameController.Singleton.StartClient();
        CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoining);
    }

    private void BackButton_OnClick()
    {
        CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoinCreate);
    }
}

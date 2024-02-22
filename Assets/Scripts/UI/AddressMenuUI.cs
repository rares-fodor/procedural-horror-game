using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows;
using System.Net;

public class AddressMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputFieldAddress;
    [SerializeField] private TMP_InputField inputFieldPort;

    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;

    [SerializeField] private TMP_Text diagnosticTextAddr;
    [SerializeField] private TMP_Text diagnosticTextPort;

    private IPAddress ipAddress;
    private ushort port;
    private bool validatedAddr = false;
    private bool validatedPort = false;

    private UnityTransport netcodeTransport;

    private void Awake()
    {
        inputFieldAddress.onEndEdit.AddListener(AddressInput_OnEndEdit);
        inputFieldPort.onEndEdit.AddListener(PortInput_OnEndEdit);
        joinButton.onClick.AddListener(JoinButton_OnClick);
        backButton.onClick.AddListener(BackButton_OnClick);

        diagnosticTextAddr.text = string.Empty;
        diagnosticTextPort.text = string.Empty;
    }

    private void Start()
    {
        netcodeTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    }

    private void AddressInput_OnEndEdit(string input)
    {
        // Fires when pressing either Enter buttons, Esc or when clicking out of the input field
        validatedAddr = IPAddress.TryParse(input, out ipAddress) && ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
    }

    private void PortInput_OnEndEdit(string input)
    {
        validatedPort = ushort.TryParse(input, out port);
    }

    private void JoinButton_OnClick()
    {
        if (!validatedAddr)
        {
            diagnosticTextAddr.text = "Address is not valid, try again";
            return;
        }
        if (!validatedPort)
        {
            diagnosticTextPort.text = "Port is not valid, try again";
            return;
        }

        Debug.Log($"Connecting to: {ipAddress}:{port}");
        netcodeTransport.SetConnectionData(ipAddress.ToString(), port);

        NetworkGameController.Singleton.StartClient();
        CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoining);
    }

    private void BackButton_OnClick()
    {
        CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyJoinCreate);
    }
}

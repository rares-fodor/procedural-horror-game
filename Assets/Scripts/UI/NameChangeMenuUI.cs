using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NameChangeMenuUI : MonoBehaviour
{
    public static NameChangeMenuUI Singleton { get; private set; }

    [SerializeField] private TMP_Text diagnosticText;
    [SerializeField] private TMP_InputField nameChangeField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;

    private string nickname;

    private void Awake()
    {
        Singleton = this;
        diagnosticText.text = string.Empty;

        nameChangeField.onEndEdit.AddListener(NameChange_OnEndEdit);

        confirmButton.onClick.AddListener(() =>
        {
            NetworkGameController.Singleton.PlayerNameChangeServerRpc(NetworkManager.Singleton.LocalClientId, nickname);
        });

        backButton.onClick.AddListener(() =>
        {
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyMain);
        });
    }

    private void NameChange_OnEndEdit(string input)
    {
        nickname = input;
    }

    public void ValidateName(bool valid)
    {
        if (valid)
        {
            CanvasController.Singleton.SetActiveScreen(CanvasController.UIScreen.LobbyMain);
        }
        else
        {
            diagnosticText.text = "Name is already taken, please type another";
        }
    }
}

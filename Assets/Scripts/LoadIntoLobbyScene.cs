using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadIntoLobbyScene : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }
}

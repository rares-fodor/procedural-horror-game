using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


// Copyright Unity Technologies 2023
// Source: https://docs-multiplayer.unity3d.com/netcode/1.5.2/basics/scenemanagement/using-networkscenemanager/


public class GameSceneController : MonoBehaviour
{

#if UNITY_EDITOR
    public UnityEditor.SceneAsset MenuSceneAsset;
    public UnityEditor.SceneAsset GameSceneAsset;
    private void OnValidate()
    {
        if (GameSceneAsset != null) { gameSceneName = GameSceneAsset.name; }
        if (menuSceneName != null) { menuSceneName = MenuSceneAsset.name; }
    }
#endif

    [SerializeField] public string gameSceneName;
    [SerializeField] public string menuSceneName;

    public static GameSceneController Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        } else if (Singleton != this)
        {
            Destroy(gameObject);
        }
    }

    public void LoadGameScene()
    {
        Debug.Log("loading game scene");
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Scene {gameSceneName} failed with status: {status}");
            }
        }
    }
    public void LoadLobbyScene()
    {
        Debug.Log("loading lobby scene");
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            var status = NetworkManager.Singleton.SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Scene {menuSceneName} failed with status: {status}");
            }
        }
    }
}

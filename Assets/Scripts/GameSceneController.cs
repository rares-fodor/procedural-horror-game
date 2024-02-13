using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


// Copyright Unity Technologies 2023
// Source: https://docs-multiplayer.unity3d.com/netcode/1.5.2/basics/scenemanagement/using-networkscenemanager/


public class GameSceneController : NetworkBehaviour
{

#if UNITY_EDITOR
    public UnityEditor.SceneAsset GameSceneAsset;
    private void OnValidate()
    {
        if (GameSceneAsset != null) { gameSceneName = GameSceneAsset.name; }
    }
#endif

    [SerializeField] private string gameSceneName;

    public void LoadScene(string sceneName)
    {
        if (IsServer && string.IsNullOrEmpty(gameSceneName))
        {
            var status = NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogWarning($"Scene {gameSceneName} failed with status: {status}");
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public static CanvasController Singleton { get; private set; }

    public enum UIScreen
    {
        LobbyJoining,
        LobbyJoinCreate,
        LobbyMain,
        LobbyAddressInput,
        // Append more here for new scenes use SceneScreen name convention
    }

    [System.Serializable]
    public struct ScreenReference
    {
        public UIScreen screen;
        public GameObject screenObject;
    }


    [SerializeField] private List<ScreenReference> screenReferences;
    private Dictionary<UIScreen, GameObject> screenDictionary;


    private void Awake()
    {
        screenDictionary = new Dictionary<UIScreen, GameObject>();
        Singleton = this;

        foreach (ScreenReference reference in screenReferences)
        {
            if (!screenDictionary.ContainsKey(reference.screen))
            {
                screenDictionary.Add(reference.screen, reference.screenObject);
            }
            else
            {
                Debug.LogWarning($"Duplicate reference found for {reference.screen}");
            }
        }
    }

    public void SetActiveScreen(UIScreen screen)
    {
        if (screenDictionary == null || screenDictionary.Count == 0)
        {
            Debug.LogError("UI screen dictionary is not populated");
        }
        if (!screenDictionary.ContainsKey(screen))
        {
            Debug.LogError($"UI screen dictionary doesn't contain entry {screen}, is it associated and does it belong to this scene?");
        }

        foreach (var kvp in screenDictionary)
        {
            if (kvp.Key == screen)
            {
                kvp.Value.SetActive(true);
            }
            else
            {
                kvp.Value.SetActive(false);
            }
        }
    }
}

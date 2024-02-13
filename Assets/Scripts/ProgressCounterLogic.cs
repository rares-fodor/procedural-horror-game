using TMPro;
using UnityEngine;

public class ProgressCounterLogic : MonoBehaviour
{
    [SerializeField] private TMP_Text progressCounterText;

    private int progress = 0;

    private void Awake()
    {
        GameController.GameProgressedEvent.AddListener(OnGameProgressed);
    }

    private void OnGameProgressed()
    {
        Debug.Log("[DEBUG] Game progressed");
        progress++;
        progressCounterText.text = $"{progress} / {Consts.PILLAR_COUNT}";
    }
}

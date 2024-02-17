using TMPro;
using UnityEngine;

public class ProgressCounterController : MonoBehaviour
{
    [SerializeField] private TMP_Text progressCounterText;

    private void Awake()
    {
        progressCounterText.text = string.Empty;
    }
    public void DisplayGameProgress(int progress)
    {
        Debug.Log("[DEBUG] Game progressed");
        progressCounterText.text = $"{progress} / {Consts.PILLAR_COUNT}";
    }
}

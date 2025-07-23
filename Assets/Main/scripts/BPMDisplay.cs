using UnityEngine;
using TMPro;

public class BPMDisplay : MonoBehaviour
{
    private TextMeshPro bpmText;
    private GlobalSequencerClock clock;

    void Start()
    {
        bpmText = GetComponent<TextMeshPro>();
        if (bpmText == null)
            Debug.LogError("BPMDisplay: no TextMeshPro component found on this GameObject.");

        clock = GlobalSequencerClock.Instance;
        if (clock == null)
            Debug.LogError("BPMDisplay: GlobalSequencerClock instance not found!");
    }

    void Update()
    {
        if (bpmText != null && clock != null)
        {
            bpmText.text = $"BPM: {clock.bpm:F0}";
        }
    }
}

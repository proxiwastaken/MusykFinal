using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class MasterVolumeControl : MonoBehaviour
{
    [Tooltip("Initial master volume (0.0 to 1.0).")]
    public float initialVolume = 1.0f;

    // Reference to the chord sequencer so we can update its masterVolume.
    [HideInInspector]
    public SynthChordSequencer sequencer;

    // Current master volume value.
    private float currentVolume;

    // Optional TextMeshPro to display the volume percentage.
    private TMP_Text volumeLabel;

    void Start()
    {
        // Initialize
        currentVolume = initialVolume;

        // Find the sequencer if not assigned
        if (sequencer == null)
            sequencer = FindObjectOfType<SynthChordSequencer>();

        // Update the sequencer’s master volume
        if (sequencer != null)
            sequencer.masterVolume = currentVolume;

        // Locate an optional TMP label
        volumeLabel = GetComponentInChildren<TMP_Text>();
        UpdateLabel();
    }

    /// <summary>
    /// Call this to raise the master volume by 'amount'.
    /// </summary>
    public void IncreaseVolume(float amount)
    {
        currentVolume = Mathf.Clamp01(currentVolume + amount);
        if (sequencer != null)
            sequencer.masterVolume = currentVolume;
        UpdateLabel();
    }

    /// <summary>
    /// Call this to lower the master volume by 'amount'.
    /// </summary>
    public void DecreaseVolume(float amount)
    {
        currentVolume = Mathf.Clamp01(currentVolume - amount);
        if (sequencer != null)
            sequencer.masterVolume = currentVolume;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (volumeLabel != null)
            volumeLabel.text = $"Vol: {(int)(currentVolume * 100)}%";
    }
}

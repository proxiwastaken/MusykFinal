using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class VolumeControl : MonoBehaviour
{
    [Tooltip("The row index this volume control corresponds to.")]
    public int rowIndex;

    [Tooltip("Initial volume (0.0 to 1.0) for this row.")]
    public float initialVolume = 1.0f;

    // Reference to the VRSequencer so we can update its rowVolumes array.
    [HideInInspector]
    public VRSequencer sequencer;

    // This value will be used to update the row volume.
    private float currentVolume;

    // Optionally, add a TextMeshPro component to display the volume value.
    private TMP_Text volumeLabel;

    void Start()
    {
        currentVolume = initialVolume;
        // Update the sequencer’s volume for this row.
        if (sequencer != null && sequencer.rowVolumes != null && rowIndex < sequencer.rowVolumes.Length)
        {
            sequencer.rowVolumes[rowIndex] = currentVolume;
        }
        // Find a TMP_Text child if available to display the volume.
        volumeLabel = GetComponentInChildren<TMP_Text>();
        UpdateLabel();
    }

    // Example method: call this when the user increases the volume.
    public void IncreaseVolume(float amount)
    {
        currentVolume = Mathf.Clamp01(currentVolume + amount);
        if (sequencer != null && sequencer.rowVolumes != null && rowIndex < sequencer.rowVolumes.Length)
        {
            sequencer.rowVolumes[rowIndex] = currentVolume;
        }
        UpdateLabel();
    }

    // Example method: call this when the user decreases the volume.
    public void DecreaseVolume(float amount)
    {
        currentVolume = Mathf.Clamp01(currentVolume - amount);
        if (sequencer != null && sequencer.rowVolumes != null && rowIndex < sequencer.rowVolumes.Length)
        {
            sequencer.rowVolumes[rowIndex] = currentVolume;
        }
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (volumeLabel != null)
        {
            volumeLabel.text = $"Vol: {(int)(currentVolume * 100)}";
        }
    }
}

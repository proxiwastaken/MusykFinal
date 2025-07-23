using UnityEngine;

public class SoundDropZone : MonoBehaviour
{
    [Tooltip("The sequencer row index that this drop zone represents.")]
    public int rowIndex;

    private VRSequencer sequencer;

    void Start()
    {
        // Assumes there is one VRSequencer in the scene.
        sequencer = FindObjectOfType<VRSequencer>();
    }

    // When a SoundItem enters the drop zone, assign its sound to the corresponding row.
    void OnTriggerEnter(Collider other)
    {
        SoundItem soundItem = other.GetComponent<SoundItem>();
        if (soundItem != null && sequencer != null)
        {
            sequencer.AssignSoundToRow(rowIndex, soundItem.AudioClip);
            Debug.Log("Assigned sound " + soundItem.AudioClip.name + " to row " + rowIndex);
        }
    }
}

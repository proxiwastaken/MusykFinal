using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BPMButton : MonoBehaviour
{
    [Tooltip("Amount to adjust BPM when this button is pressed. (Use negative values for decrease.)")]
    public float delta = 1f;

    public VRSequencer sequencer;  // Assign via Inspector or find in Start()

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Start()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable != null)
        {
            // Listen for the press event.
            interactable.activated.AddListener(OnButtonPressed);
        }
        else
        {
            Debug.LogError("BPM needs a Base Interactable!");
        }
    }

    void OnButtonPressed(ActivateEventArgs args)
    {
        var clock = GlobalSequencerClock.Instance;
        if (clock != null)
        {
            clock.bpm += delta;
            clock.bpm = Mathf.Clamp(clock.bpm, 30f, 300f);
            Debug.Log($"Global BPM adjusted by {delta}, new BPM = {clock.bpm}");
        }
        else
        {
            Debug.LogError("GlobalSequencerClock instance not found!");
        }
    }
}

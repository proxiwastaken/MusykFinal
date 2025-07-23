using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ChordButton : MonoBehaviour
{
    // The chord type this button represents (e.g., "Major").
    public string chordType;

    // Delegate callback to notify when this button is pressed.
    public System.Action<string> OnButtonPressed;

    // The current key (e.g., "C") for display purposes.
    public string currentKey;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        }
    }

    void OnEnable()
    {
        interactable.activated.AddListener(HandleSelectEntered);
    }

    void OnDisable()
    {
        interactable.activated.RemoveListener(HandleSelectEntered);
    }

    void HandleSelectEntered(ActivateEventArgs args)
    {
        if (OnButtonPressed != null)
            OnButtonPressed.Invoke(chordType);
    }
}

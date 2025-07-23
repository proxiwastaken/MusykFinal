using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class KeyButton : MonoBehaviour
{
    // The value of the button (e.g., a chord type like "Major" or a key like "C").
    public string value;

    // A delegate callback to notify when this button is pressed.
    public System.Action<string> OnButtonPressed;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (interactable == null)
        {
            // Add XRGrabInteractable if missing.
            interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
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
        // Call the delegate to notify the palette manager.
        if (OnButtonPressed != null)
            OnButtonPressed.Invoke(value);
    }
}

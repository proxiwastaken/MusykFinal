using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SoundSlot : MonoBehaviour
{
    [Tooltip("The sequencer row index this slot corresponds to.")]
    public int rowIndex;

    [Tooltip("Default audio clip for this slot if no item is captured.")]
    public AudioClip defaultAudioClip;

    private AudioClip assignedClip;
    private bool hasItem = false;
    private SoundItem capturedItem = null;

    private Renderer slotRenderer;
    private Color originalColor;
    public VRSequencer sequencer;

    // Maximum distance from the slot's center to capture the item.
    public float maxCaptureDistance = 0.05f;

    // Colors: hover (blue) and captured (green)
    public Color hoverColor = new Color(0f, 0.5f, 1f, 0.41f);
    public Color capturedColor = new Color(0f, 1f, 0f, 0.41f);

    void Start()
    {
        sequencer = FindObjectOfType<VRSequencer>();
        slotRenderer = GetComponent<Renderer>();
        if (slotRenderer != null)
            originalColor = slotRenderer.material.color;
        assignedClip = defaultAudioClip;
        if (sequencer != null)
            sequencer.AssignSoundToRow(rowIndex, assignedClip);
    }

    void OnTriggerEnter(Collider other)
    {
        SoundItem item = other.GetComponent<SoundItem>();
        if (item == null || hasItem)
            return;

        // If the item is held, just show hover (blue).
        if (item.IsBeingHeld())
        {
            SetSlotColor(hoverColor);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (hasItem)
            return;

        SoundItem item = other.GetComponent<SoundItem>();
        if (item == null)
            return;

        // If the item is held, keep the slot hover-blue.
        if (item.IsBeingHeld())
        {
            SetSlotColor(hoverColor);
            return;
        }

        // If not held, check if it's close enough.
        float distance = Vector3.Distance(item.transform.position, transform.position);
        if (distance <= maxCaptureDistance)
        {
            CaptureItem(item);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!hasItem)
            SetSlotColor(originalColor);
    }

    public void ForceCapture(SoundItem item)
    {
        CaptureItem(item);
    }

    void CaptureItem(SoundItem item)
    {
        if (hasItem)
            return;

        Debug.Log("Capturing SoundItem: " + item.name + " in slot for row " + rowIndex);
        assignedClip = item.AudioClip; // Using the property from SoundItem.
        hasItem = true;
        capturedItem = item;

        // Cancel the item's return behavior.
        item.DisableReturn();

        // Snap the item to the slot's position and rotation.
        item.transform.position = transform.position;
        item.transform.rotation = transform.rotation;
        // Parent the item to the slot.
        item.transform.SetParent(transform);

        // Freeze the item: disable gravity and make it kinematic.
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.useGravity = false;
            itemRb.isKinematic = true;
        }

        // Update the sequencer row's sound.
        if (sequencer != null)
            sequencer.AssignSoundToRow(rowIndex, assignedClip);

        // Change the slot's appearance to captured (green).
        SetSlotColor(capturedColor);

        // Subscribe to the item's grab event so that when it is picked up, the slot releases it.
        XRGrabInteractable interactable = item.GetComponent<XRGrabInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnCapturedItemGrabbed);
        }
    }

    // This callback is invoked when the captured item is grabbed.
    void OnCapturedItemGrabbed(SelectEnterEventArgs args)
    {
        if (capturedItem != null)
        {
            ReleaseCapturedItem();
        }
    }

    /// <summary>
    /// Releases the captured SoundItem by re-parenting it back to the SoundPanel.
    /// Also clears the slot's assignment.
    /// </summary>
    public void ReleaseCapturedItem()
    {
        if (!hasItem || capturedItem == null)
            return;

        Debug.Log("Releasing captured SoundItem from slot row " + rowIndex);

        // Reparent the captured item back to the SoundPanel.
        if (sequencer != null && sequencer.soundPanel != null)
        {
            capturedItem.transform.SetParent(sequencer.soundPanel);
        }
        else
        {
            // Fallback (should not occur if soundPanel is assigned)
            capturedItem.transform.SetParent(null);
        }

        // Re-enable its return behavior and force it to return to its original grid position.
        capturedItem.EnableReturn();
        capturedItem.ForceReturnToGrid();

        // Clear the slot's stored data.
        hasItem = false;
        assignedClip = null;
        SetSlotColor(originalColor);
        if (sequencer != null)
            sequencer.AssignSoundToRow(rowIndex, assignedClip);

        // Unsubscribe from the captured item's grab event.
        XRGrabInteractable interactable = capturedItem.GetComponent<XRGrabInteractable>();
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnCapturedItemGrabbed);

        capturedItem = null;
    }

    private void SetSlotColor(Color color)
    {
        if (slotRenderer != null)
            slotRenderer.material.color = color;
    }
}

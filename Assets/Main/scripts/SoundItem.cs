using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SoundItem : MonoBehaviour
{
    private AudioClip _audioClip;
    public AudioClip AudioClip
    {
        get { return _audioClip; }
        set
        {
            _audioClip = value;
            UpdateLabel();
        }
    }

    public float returnDelay = 3f;

    public Transform soundPanel;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Coroutine returnCoroutine;
    public bool isDefault = false;
    private bool isHeld = false;
    private bool returnDisabled = false;

    [HideInInspector]
    public bool justRemovedFromSlot = false;

    // Cache a preview AudioSource.
    private AudioSource previewAudioSource;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            Debug.LogError("XRGrabInteractable missing on SoundItem.");
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        // Ensure an AudioSource is attached for sound preview.
        previewAudioSource = GetComponent<AudioSource>();
        if (previewAudioSource == null)
            previewAudioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        if (isDefault)
            returnDisabled = true;

        rb.useGravity = false;
        rb.isKinematic = true;
        UpdateLabel();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    void UpdateLabel()
    {
        TMP_Text label = GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = _audioClip != null ? _audioClip.name : "No Clip";
        }
    }

    public bool IsBeingHeld()
    {
        return isHeld;
    }

    public void DisableReturn()
    {
        returnDisabled = true;
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }

    public void EnableReturn()
    {
        if (isDefault)
        {
            return;
        } else
        {
            returnDisabled = false;
        }
        
    }

    public void ForceReturnToGrid()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        rb.useGravity = false;
        rb.isKinematic = true;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }

    public void PermanentlyDisableReturn()
    {
        // Prevent any future returns:
        returnDisabled = true;

        // Kill any in-flight coroutine:
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        // Unsubscribe your exit listener so OnSelectExited never restarts it:
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            initialPosition = Vector3.zero;
            initialRotation = Quaternion.identity;
            initialLocalPosition = Vector3.zero;
            initialLocalRotation = Quaternion.identity;
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        isHeld = true;
        // If the item is parented to a slot, let the slot handle removal (our SoundSlot subscription does that)
        if (transform.parent != null)
        {
            // Detach from the slot.
            transform.SetParent(null);
            // Set a cooldown flag to avoid immediate recapture.
            StartCoroutine(ClearJustRemovedFlag());
        }
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        rb.useGravity = false;
        rb.isKinematic = true;

        // Play the preview AudioClip when the item is picked up.
        if (AudioClip != null)
        {
            // Optionally, adjust volume or other settings.
            previewAudioSource.PlayOneShot(AudioClip);
        }
    }

    IEnumerator ClearJustRemovedFlag()
    {
        justRemovedFromSlot = true;
        yield return new WaitForSeconds(0.5f);
        justRemovedFromSlot = false;
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        isHeld = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        if (!returnDisabled)
        {
            returnCoroutine = StartCoroutine(ReturnToInitialPosition());
        }
    }

    IEnumerator ReturnToInitialPosition()
    {
        yield return new WaitForSeconds(returnDelay);
        if (!isHeld && !returnDisabled)
        {
            // Reparent the item back to the soundPanel if it isn't already.
            if (soundPanel != null && transform.parent != soundPanel)
            {
                transform.SetParent(soundPanel);
            }

            rb.useGravity = false;
            rb.isKinematic = true;
            float duration = 1f;
            float elapsed = 0f;
            Vector3 startLocalPos = transform.localPosition;
            Quaternion startLocalRot = transform.localRotation;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(startLocalPos, initialLocalPosition, elapsed / duration);
                transform.localRotation = Quaternion.Slerp(startLocalRot, initialLocalRotation, elapsed / duration);
                yield return null;
            }
            transform.localPosition = initialLocalPosition;
            transform.localRotation = initialLocalRotation;
        }
    }
}

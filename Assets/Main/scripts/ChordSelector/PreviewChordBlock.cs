using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;

public class PreviewChordBlock : MonoBehaviour
{
    [Header("Chord Data")]
    public AudioClip chordSound;   // The chord sample (often a default synth sound)
    public string chordKey = "C";  // For example, "C"
    public string chordType = "Major";  // For example, "Major"

    [Header("Instrument Settings")]
    public int instrumentIndex = 0;   // Which instrument from your bank to use
    public string[] instrumentNames;
    // Computed from chordKey below:
    public int rootNoteIndex { get; private set; } = 0;

    public InstrumentBank instrumentBank;

    private TMP_Text previewLabel;
    private AudioSource previewAudioSource;
    private readonly string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    public float returnDelay = 3f;
    public Transform soundPanel;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Coroutine returnCoroutine;
    private bool isHeld = false;
    private bool returnDisabled = false;

    [HideInInspector]
    public bool justRemovedFromSlot = false;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
            Debug.LogError("Grab Interactable not on PreviewBlock");

        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        previewLabel = GetComponentInChildren<TMP_Text>();
        previewAudioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        previewAudioSource.playOnAwake = false;

        grabInteractable.activated.AddListener(OnActivated);
    }

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        UpdateRootNoteIndex();
        UpdateDisplay();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);
    }

    /// <summary>
    /// Updates the on-block display with the current chord selections.
    /// </summary>
    public void UpdateDisplay()
    {
        if (previewLabel != null)
        {
            string instLabel = (instrumentNames != null
                        && instrumentIndex >= 0
                        && instrumentIndex < instrumentNames.Length)
                       ? instrumentNames[instrumentIndex]
                       : $"Inst {instrumentIndex}";

            previewLabel.text = $"{chordKey}\n{chordType}\n{instLabel}";
        }
    }

    /// <summary>
    /// Sets the chord data for this preview block.
    /// </summary>
    /// <param name="key">Chord key (e.g. "C")</param>
    /// <param name="type">Chord type (e.g. "Major")</param>
    /// <param name="instrIndex">Instrument index (0–19)</param>
    /// <param name="sound">Optional AudioClip (default synth sample)</param>
    public void SetChord(string key, string type, int instrIndex, AudioClip sound = null)
    {
        chordKey = key;
        chordType = type;
        instrumentIndex = instrIndex;
        if (sound != null)
            chordSound = sound;
        UpdateRootNoteIndex();
        UpdateDisplay();
    }

    /// <summary>
    /// Computes the root note index from chordKey.
    /// </summary>
    private void UpdateRootNoteIndex()
    {
        for (int i = 0; i < noteNames.Length; i++)
        {
            if (string.Equals(chordKey, noteNames[i], System.StringComparison.OrdinalIgnoreCase))
            {
                rootNoteIndex = i;
                return;
            }
        }
        rootNoteIndex = 0; // default if not found.
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
        returnDisabled = false;
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

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        isHeld = true;

        if (transform.parent != null)
        {
            transform.SetParent(null);
            StartCoroutine(ClearJustRemovedFlag());
        }
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
        rb.useGravity = false;
        rb.isKinematic = true;
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

    void OnActivated(ActivateEventArgs args)
    {
        if (instrumentBank == null)
        {
            Debug.LogError("PreviewChordBlock: InstrumentBank not set!");
            return;
        }

        // pick intervals
        int[] intervals;
        switch (chordType.ToLower())
        {
            case "minor": intervals = new[] { 0, 3, 7 }; break;
            case "diminished": intervals = new[] { 0, 3, 6 }; break;
            case "suspended":
            case "sus": intervals = new[] { 0, 5, 7 }; break;
            case "major":
            default: intervals = new[] { 0, 4, 7 }; break;
        }

        float volPerNote = 1f / intervals.Length;

        // for each interval, look up the clip and play it
        foreach (int iv in intervals)
        {
            int noteIdx = (rootNoteIndex + iv) % 12;
            AudioClip clip = instrumentBank.GetClip(instrumentIndex, noteIdx);
            if (clip != null)
                previewAudioSource.PlayOneShot(clip, volPerNote);
            else
                Debug.LogWarning($"No clip for instr {instrumentIndex}, note {noteIdx}");
        }

        Debug.Log($"Previewed chord {chordKey} {chordType}");
    }

    IEnumerator ClearJustRemovedFlag()
    {
        justRemovedFromSlot = true;
        yield return new WaitForSeconds(0.5f);
        justRemovedFromSlot = false;
    }

    IEnumerator ReturnToInitialPosition()
    {
        yield return new WaitForSeconds(returnDelay);
        if (!isHeld && !returnDisabled)
        {
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

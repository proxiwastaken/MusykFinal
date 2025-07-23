using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class ChordBlock : MonoBehaviour
{
    [Header("Grid Settings")]
    public int row;          // The grid row for this cell.
    public int startBeat;    // The beat (column index) that this cell represents.
    public int duration = 2; // Duration in beats (default: 2 beats for half a bar).

    [Header("Chord Data")]
    public AudioClip chordSound;   // The synth sample to play.
    public string chordKey = "";   // e.g., "C"
    public string chordType = "";  // e.g., "Major"

    [Header("Playback Settings")]
    public InstrumentBank instrumentBank;
    public int instrumentIndex = 0;   // Which instrument from the bank.
    public int rootNoteIndex = 0;       // Calculated from chordKey.

    // Internal flag whether a chord has been assigned.
    private bool hasChord = false;

    // Reference to a TextMeshPro component to display chord info.
    private TMP_Text chordLabel;

    void Awake()
    {
        chordLabel = GetComponentInChildren<TMP_Text>();
    }

    void Start()
    {
        // Make the cell semi-transparent.
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color c = rend.material.color;
            c.a = 0.5f;
            rend.material.color = c;
        }
        UpdateChordDisplay();
    }

    /// <summary>
    /// Updates the cell's display text.
    /// </summary>
    void UpdateChordDisplay()
    {
        if (chordLabel != null)
        {
            chordLabel.text = hasChord ? (chordKey + "\n" + chordType) : "";
        }
    }

    /// <summary>
    /// “Paints” this cell with the chord data from a PreviewChordBlock.
    /// This method copies the preview’s chord data without reparenting or scaling the preview block.
    /// It simply updates this cell’s data.
    /// </summary>
    public void AssignChordFromPreview(PreviewChordBlock preview)
    {
        if (preview == null)
            return;
        hasChord = true;
        chordKey = preview.chordKey;
        chordType = preview.chordType;
        chordSound = preview.chordSound;
        instrumentIndex = preview.instrumentIndex;
        rootNoteIndex = preview.rootNoteIndex;
        UpdateChordDisplay();
        Debug.Log("ChordBlock assigned chord: " + chordKey + " " + chordType);
    }

    /// <summary>
    /// Clears the assigned chord data.
    /// </summary>
    public void ClearAssignedChord()
    {
        hasChord = false;
        chordKey = "";
        chordType = "";
        chordSound = null;
        UpdateChordDisplay();
    }

    /// <summary>
    /// Returns true if this cell has a chord assigned.
    /// </summary>
    public bool HasChord()
    {
        return hasChord;
    }

    /// <summary>
    /// Plays the chord by looking up samples in the InstrumentBank.
    /// Uses the chord's instrumentIndex and rootNoteIndex along with chordType intervals.
    /// </summary>
    public void PlayChord(double dspScheduledTime, float volume)
    {
        // Check that instrumentBank has been assigned.
        if (instrumentBank == null)
        {
            Debug.LogError("InstrumentBank not assigned to ChordBlock!");
            return;
        }
        // Ensure the instrument exists.
        if (instrumentIndex < 0 || instrumentIndex >= instrumentBank.instruments.Count)
        {
            Debug.LogError("Invalid instrument index in ChordBlock.");
            return;
        }

        int[] intervals;
        switch (chordType.ToLower())
        {
            case "minor":
                intervals = new int[] { 0, 3, 7 };
                break;
            case "diminished":
                intervals = new int[] { 0, 3, 6 };
                break;
            case "suspended":
            case "sus":
                intervals = new int[] { 0, 5, 7 }; // sus4 chord.
                break;
            case "major":
            default:
                intervals = new int[] { 0, 4, 7 };
                break;
        }

        int noteCount = intervals.Length;

        float individualVolume = 1.0f / noteCount;

        // Lookup and play each note in the chord.
        foreach (int interval in intervals)
        {
            int noteIndex = (rootNoteIndex + interval) % 12;
            AudioClip clip = instrumentBank.GetClip(instrumentIndex, noteIndex);
            if (clip != null)
            {
                // Create a one‑shot AudioSource for this note
                var src = gameObject.AddComponent<AudioSource>();
                src.clip = clip;
                src.volume = individualVolume * volume;
                src.playOnAwake = false;
                src.spatialBlend = 1f;
                src.SetScheduledStartTime(dspScheduledTime);
                src.PlayScheduled(dspScheduledTime);

                // destroy this AudioSource component after the clip finishes
                Destroy(src, clip.length + 0.1f);
            }
            else
            {
                Debug.LogWarning("No clip found for instrument " + instrumentIndex + " at note " + noteIndex);
            }
        }
    }

        /// <summary>
        /// Changes the cell's visual highlight.
        /// </summary>
        public void SetHighlight(bool highlight)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color baseColor = Color.white;
            baseColor.a = 0.5f;
            if (highlight)
                rend.material.color = Color.yellow;
            else
                rend.material.color = hasChord ? baseColor * 0.8f : baseColor;
        }
    }

    /// <summary>
    /// Trigger method: When a preview block collides with this cell, assign its chord data.
    /// We want the preview block to act like a paintbrush so we only copy data.
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        // 1) If it’s our Preview “paintbrush,” assign the chord...
        if (other.TryGetComponent<PreviewChordBlock>(out var preview))
        {
            AssignChordFromPreview(preview);
            return;
        }

        // 2) If it’s the ClearBrush, wipe this cell clean
        if (other.TryGetComponent<ClearBrush>(out _))
        {
            ClearAssignedChord();
        }

    }
}

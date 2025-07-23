using System.Collections;
using UnityEngine;

public class SynthChordSequencer : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 1;
    public int columns = 16;  // One column per beat.
    // gridStartPosition is the top-left corner of the grid in world space.
    public Vector3 gridStartPosition = new Vector3(0, 1, 0);
    public float spacing = 0.02f;
    // These BPM-related fields are now controlled by GlobalSequencerClock.
    public float bpm = 120f;
    public int stepsPerBeat = 2;

    public float masterVolume = 1.0f;

    private int chordBlockDuration;

    [Header("Prefabs & Panel")]
    public GameObject chordBlockPrefab;  // Your ChordBlock prefab (drop target, semi-transparent).
    public Transform chordGridPanel;     // The container for the chord grid.

    [Header("Sound Panel")]
    public Transform soundPanel;         // The panel that holds preview chord blocks (for re-parenting).

    public InstrumentBank instrumentBank;

    private int blockCount;
    private int currentBlock = 0;

    private int globalStep = 0;
    private int offset;

    private ChordBlock[] grid;

    void Start()
    {
        chordBlockDuration = 2 * stepsPerBeat;
        offset = stepsPerBeat - 1;
        blockCount = columns / chordBlockDuration;
        CreateGrid();
        // No need to start our own coroutine when using a global clock.
    }

    void OnEnable()
    {
        if (GlobalSequencerClock.Instance != null)
        {
            Debug.Log("SynthChordSequencer subscribing to global clock...");
            GlobalSequencerClock.Instance.OnBeat += HandleBeat;
        }
        else
        {
            Debug.LogError("GlobalSequencerClock instance is null!");
        }
    }

    void OnDisable()
    {
        if (GlobalSequencerClock.Instance != null)
            GlobalSequencerClock.Instance.OnBeat -= HandleBeat;
    }

    /// <summary>
    /// Handles a beat event from the global clock.
    /// For each beat, it checks each grid cell—if a cell has an assigned chord (via a stored preview),
    /// and if the current beat equals its startBeat, it triggers playback.
    /// It then highlights the current column.
    /// </summary>
    void HandleBeat(double dspScheduledTime)
    {
        globalStep++;
        Debug.Log("GlobalStep: " + globalStep);

        // Only trigger chord playback when (globalStep - offset) mod chordBlockDuration is zero.
        if ((globalStep - offset) % chordBlockDuration != 0)
            return;

        // Calculate the active block index.
        currentBlock = ((globalStep - offset) / chordBlockDuration) % blockCount;
        Debug.Log("Triggering chord block index: " + currentBlock);

        // Trigger playback if that cell has chord data.
        if (grid[currentBlock] != null && grid[currentBlock].HasChord())
        {
            grid[currentBlock].PlayChord(dspScheduledTime, masterVolume);
        }
        HighlightBlock(currentBlock);

    }

    /// <summary>
    /// Generates a grid of chord cells (drop targets) using the ChordBlock prefab.
    /// The cells are arranged vertically (rows along Y) and horizontally along the z‑axis (columns).
    /// </summary>
    void CreateGrid()
    {
        grid = new ChordBlock[blockCount];
        Vector3 worldStart = transform.position + gridStartPosition;
        for (int i = 0; i < blockCount; i++)
        {
            // Position calculation:
            // - x remains constant,
            // - y decreases with rows,
            // - z increases with columns.
            Vector3 pos = worldStart + new Vector3(0, 0, i * spacing * chordBlockDuration);
            GameObject cell = Instantiate(chordBlockPrefab, pos, Quaternion.identity, chordGridPanel);
            // Do not force scale here—ensure your prefab is sized correctly.
            ChordBlock cb = cell.GetComponent<ChordBlock>();
            if (cb != null)
            {
                cb.row = 0;
                cb.startBeat = i * chordBlockDuration + offset;
                cb.duration = chordBlockDuration;  // This means the chord should sustain for 2 beats (half a bar in a 4-beat measure).
                cb.instrumentBank = instrumentBank;
            }
            else
            {
                Debug.LogError("ChordBlock component missing on chordBlockPrefab!");
            }
            grid[i] = cb;
        }
    }

    /// <summary>
    /// Highlights the cells in the current beat column.
    /// </summary>
    void HighlightBlock(int blockIndex)
    {
        if (grid[blockIndex] != null)
            grid[blockIndex].SetHighlight(true);
        int prevIndex = (blockIndex == 0) ? blockCount - 1 : blockIndex - 1;
        if (grid[prevIndex] != null)
            grid[prevIndex].SetHighlight(false);
    }
}

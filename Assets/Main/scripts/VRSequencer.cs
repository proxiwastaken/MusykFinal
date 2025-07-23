using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRSequencer : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 4;
    public int columns = 8;
    public GameObject blockPrefab;          
    public Vector3 gridStartPosition = new Vector3(0, 1, 0);
    public float spacing = 0.2f;
    public AudioClip[] audioClips;           // Default sounds for each row
    public float bpm = 120f;
    public int stepsPerBeat = 2;

    [Header("Panel & Controls")]
    public Transform soundPanel;             // Holds SoundItems (the SoundPanel)
    public GameObject soundSlotPrefab;       // Your SoundSlot prefab
    public GameObject volumeControlPrefab;   // Your VolumeControl prefab
    public GameObject defaultSoundItemPrefab;

    [Header("Runtime Data")]
    public float[] rowVolumes;               // Volume for each row, default is 1.0

    private GameObject[,] grid;
    private AudioSource audioSource;
    private int currentStep = 0;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.maxDistance = 10f;
        audioSource.minDistance = 0f;

        var curve = new AnimationCurve(
            new Keyframe(0f, 0.6f),  // 60% loud right on top
            new Keyframe(1f, 0f)     // 0% at max Distance
        );
        audioSource.SetCustomCurve(
            AudioSourceCurveType.CustomRolloff,
            curve
        );
        CreateGrid();
        CreateSoundSlotsForRows();
        CreateVolumeControlsForRows();
        // Remove local timing coroutine; we now use the global clock.
        //StartCoroutine(PlaySequence());
    }

    void OnEnable()
    {
        if (GlobalSequencerClock.Instance != null)
            GlobalSequencerClock.Instance.OnBeat += HandleBeat;
    }

    void OnDisable()
    {
        if (GlobalSequencerClock.Instance != null)
            GlobalSequencerClock.Instance.OnBeat -= HandleBeat;
    }

    /// <summary>
    /// Called on every beat by the GlobalSequencerClock.
    /// It highlights the current column, plays sounds for that column, then increments currentStep.
    /// </summary>
    private void HandleBeat(double _)
    {
        HighlightColumn(currentStep);
        PlaySoundsForColumn(currentStep);
        currentStep = (currentStep + 1) % columns;
    }

    /// <summary>
    /// Assigns the given AudioClip to a specific row.
    /// </summary>
    public void AssignSoundToRow(int row, AudioClip clip)
    {
        if (row >= 0 && row < audioClips.Length)
        {
            audioClips[row] = clip;
            if (clip != null)
                Debug.Log("Row " + row + " now uses sound: " + clip.name);
            else
                Debug.Log("Row " + row + " has no sound assigned.");
        }
        else
        {
            Debug.LogWarning("Row index " + row + " is out of range.");
        }
    }

    /// <summary>
    /// Creates the grid of blocks.
    /// </summary>
    void CreateGrid()
    {
        grid = new GameObject[rows, columns];

        // Compute worldStart as this.transform.position + gridStartPosition.
        Vector3 worldStart = transform.position + gridStartPosition;

        // Initialize rowVolumes array.
        rowVolumes = new float[rows];
        for (int r = 0; r < rows; r++)
            rowVolumes[r] = 0.2f;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculate block position relative to worldStart.
                Vector3 position = worldStart + new Vector3(col * spacing, -row * spacing, 0);
                // Instantiate blockPrefab; parent to this GameObject.
                GameObject block = Instantiate(blockPrefab, position, Quaternion.identity, transform);
                block.transform.localScale = Vector3.one * 0.1f;

                grid[row, col] = block;

                BlockInteraction blockInteraction = block.GetComponent<BlockInteraction>();
                if (blockInteraction == null)
                {
                    Debug.LogError("BlockInteraction component missing on blockPrefab!");
                }
                else
                {
                    blockInteraction.Setup(row, col, OnBlockToggled);
                }
            }
        }
    }

    /// <summary>
    /// Creates SoundSlots for each row (positioned to the left of the first block).
    /// </summary>
    void CreateSoundSlotsForRows()
    {
        for (int row = 0; row < rows; row++)
        {
            GameObject slot = Instantiate(soundSlotPrefab, transform);
            slot.name = "SoundSlot_Row" + row;

            Vector3 firstBlockPos = grid[row, 0].transform.position;
            Vector3 slotPos = firstBlockPos + new Vector3(-0.2f, 0, 0);
            slot.transform.position = slotPos;

            SoundSlot slotScript = slot.GetComponent<SoundSlot>();
            if (slotScript != null)
            {
                slotScript.rowIndex = row;
                slotScript.defaultAudioClip = audioClips[row];
                slotScript.sequencer = this;

                if (defaultSoundItemPrefab != null && slotScript.defaultAudioClip != null)
                {
                    GameObject go = Instantiate(defaultSoundItemPrefab, slot.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;

                    go.transform.localScale = blockPrefab.transform.localScale * 0.8f;

                    SoundItem si = go.GetComponent<SoundItem>();
                    si.isDefault = true;
                    si.AudioClip = slotScript.defaultAudioClip;

                    slotScript.ForceCapture(si);

                    si.DisableReturn();

                    //slotScript.defaultAudioClip = null;

                }
            }
            else
            {
                Debug.LogWarning("SoundSlot prefab is missing SoundSlot component!");
            }
        }
    }

    /// <summary>
    /// Creates VolumeControl objects for each row (positioned further left than the SoundSlot).
    /// </summary>
    void CreateVolumeControlsForRows()
    {
        for (int row = 0; row < rows; row++)
        {
            GameObject volControl = Instantiate(volumeControlPrefab, transform);
            volControl.name = "VolumeControl_Row" + row;

            Vector3 worldStart = transform.position + gridStartPosition;
            Vector3 firstBlockPos = worldStart + new Vector3(0, -row * spacing, 0);
            Vector3 volPos = firstBlockPos + new Vector3(-0.4f, 0, 0);
            volControl.transform.position = volPos;

            VolumeControl volScript = volControl.GetComponent<VolumeControl>();
            if (volScript != null)
            {
                volScript.rowIndex = row;
                volScript.initialVolume = rowVolumes[row];
                volScript.sequencer = this;
            }
            else
            {
                Debug.LogWarning("VolumeControl prefab is missing VolumeControl component!");
            }
        }
    }

    /// <summary>
    /// Called when a block is toggled (for debugging).
    /// </summary>
    void OnBlockToggled(int row, int col, bool isActive)
    {
        Debug.Log($"Block at ({row}, {col}) is now {(isActive ? "Active" : "Inactive")}");
    }

    /// <summary>
    /// Highlights the current column and un-highlights the previous column.
    /// </summary>
    void HighlightColumn(int col)
    {
        Debug.Log($"Highlighting column {col}");
        for (int row = 0; row < rows; row++)
        {
            var blockInteraction = grid[row, col].GetComponent<BlockInteraction>();
            if (blockInteraction != null)
            {
                blockInteraction.SetHighlight(true);
                Debug.Log($"Highlighted block at row {row}, column {col}");
            }
            else
            {
                Debug.LogWarning($"No BlockInteraction found at row {row}, column {col}");
            }
        }
        int previousCol = (col == 0) ? columns - 1 : col - 1;
        for (int row = 0; row < rows; row++)
        {
            var blockInteraction = grid[row, previousCol].GetComponent<BlockInteraction>();
            if (blockInteraction != null)
            {
                blockInteraction.SetHighlight(false);
                Debug.Log($"Unhighlighted block at row {row}, column {previousCol}");
            }
            else
            {
                Debug.LogWarning($"No BlockInteraction found at row {row}, column {previousCol}");
            }
        }
    }

    /// <summary>
    /// Plays the sounds for the current column, using each row's volume setting.
    /// </summary>
    void PlaySoundsForColumn(int col)
    {
        Debug.Log($"Playing sounds for column {col}");
        for (int row = 0; row < rows; row++)
        {
            var block = grid[row, col];
            var blockInteraction = block.GetComponent<BlockInteraction>();
            if (blockInteraction != null && blockInteraction.IsActive)
            {
                Debug.Log($"Playing sound for row {row}, column {col}");
                if (row < audioClips.Length && audioClips[row] != null)
                {
                    AudioSource.PlayClipAtPoint(audioClips[row], transform.position, rowVolumes[row]);
                    Debug.Log($"Sound played for row {row} with volume {rowVolumes[row]}");
                }
                else
                {
                    Debug.LogWarning($"No audio clip assigned for row {row}");
                }
            }
        }
    }

    /// <summary>
    /// Resizes the grid to new dimensions. Before destroying the current grid,
    /// this method iterates through all SoundSlots and calls ReleaseCapturedItem(),
    /// re-parenting any captured SoundItems back to the soundPanel.
    /// Then, it rebuilds the grid, sound slots, and volume controls.
    /// </summary>
    public void ResizeGrid(int newRows, int newColumns)
    {
        // Release captured SoundItems from all SoundSlots.
        SoundSlot[] slots = GetComponentsInChildren<SoundSlot>();
        foreach (SoundSlot slot in slots)
        {
            slot.ReleaseCapturedItem();
        }

        // Destroy all children (blocks, slots, volume controls, etc.).
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Update grid dimensions.
        rows = newRows;
        columns = newColumns;

        // Reinitialize rowVolumes.
        rowVolumes = new float[rows];
        for (int r = 0; r < rows; r++)
            rowVolumes[r] = 1.0f;

        // Rebuild the grid and associated elements.
        CreateGrid();
        CreateSoundSlotsForRows();
        CreateVolumeControlsForRows();

        Debug.Log("Resized grid to " + rows + " rows and " + columns + " columns.");
    }
}

using UnityEngine;
using TMPro;

public class ChordPaletteManager : MonoBehaviour
{
    [Header("Prefabs and Containers")]
    public GameObject instrumentButtonPrefab;
    public GameObject chordTypeButtonPrefab;   // Prefab for chord type buttons (with a TextMeshPro child)
    public GameObject chordKeyButtonPrefab;      // Prefab for chord key buttons (with a TextMeshPro child)
    public Transform instrumentContainer;
    public Transform chordTypeContainer;         // Parent for chord type buttons
    public Transform chordKeyContainer;          // Parent for chord key buttons
    public GameObject previewChordBlockPrefab;   // Grabbable preview chord block prefab
    public GameObject clearBrushPrefab;
    public Transform previewArea;                // Where the preview block is placed

    [Header("Chord Settings")]
    public string[] instrumentNames = { "Piano", "E. Piano"};
    public string[] chordTypes = { "Major", "Minor", "Dim", "Sus" };
    public string[] chordKeys = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    public int defaultKeyIndex = 0;
    public int defaultChordTypeIndex = 0;
    public int defaultInstrumentIndex = 0;

    // Current selections.
    private string currentChordType;
    private string currentKey;
    private int currentInstrumentIndex;

    private GameObject clearBrush;
    private GameObject previewChordBlock;

    void Start()
    {
        // Set default selections.
        currentInstrumentIndex = defaultInstrumentIndex;
        currentChordType = chordTypes[defaultChordTypeIndex];
        currentKey = chordKeys[defaultKeyIndex];

        // Instantiate the chord type and key buttons.
        CreateInstrumentButtons();
        CreateChordTypeButtons();
        CreateChordKeyButtons();

        // Instantiate the preview chord block.
        if (previewChordBlockPrefab != null && previewArea != null)
        {
            previewChordBlock = Instantiate(previewChordBlockPrefab, previewArea);
            clearBrush = Instantiate(clearBrushPrefab, previewArea);
            clearBrush.transform.localPosition = previewChordBlock.transform.localPosition + new Vector3(0f, 0f, 0.3f);
            var brushScript = clearBrush.GetComponent<ClearBrush>();
            brushScript.soundPanel = previewArea;
            UpdatePreviewDisplay();
        }
    }

    void CreateInstrumentButtons()
    {
        // Clear existing children.
        foreach (Transform child in instrumentContainer)
            Destroy(child.gameObject);

        float spacing = 0.39f;
        for (int i = 0; i < instrumentNames.Length; i++)
        {
            GameObject btn = Instantiate(instrumentButtonPrefab, instrumentContainer);
            btn.name = "Instrument_" + instrumentNames[i];
            // Arrange buttons horizontally (e.g., along the X-axis).
            btn.transform.localPosition = new Vector3(0, 0, i * spacing);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = instrumentNames[i];

            // Add a simple key button script.
            // We’ll reuse the simple KeyButton, but here the value is an integer represented as a string.
            KeyButton kb = btn.AddComponent<KeyButton>();
            kb.value = i.ToString();
            kb.OnButtonPressed = OnInstrumentSelected;
        }
    }

    void CreateChordTypeButtons()
    {
        // Clear existing children.
        foreach (Transform child in chordTypeContainer)
            Destroy(child.gameObject);

        float spacing = 0.28f;
        for (int i = 0; i < chordTypes.Length; i++)
        {
            GameObject btn = Instantiate(chordTypeButtonPrefab, chordTypeContainer);
            btn.name = "ChordType_" + chordTypes[i];
            btn.transform.localPosition = new Vector3(0, 0, i * spacing);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = chordTypes[i];

            // Add an interaction script.
            KeyButton keyButton = btn.AddComponent<KeyButton>();
            keyButton.value = chordTypes[i];
            keyButton.OnButtonPressed = OnChordTypeSelected;
        }
    }

    void CreateChordKeyButtons()
    {
        foreach (Transform child in chordKeyContainer)
            Destroy(child.gameObject);

        // Layout keys in one row.
        float spacing = 0.15f;
        for (int i = 0; i < chordKeys.Length; i++)
        {
            GameObject btn = Instantiate(chordKeyButtonPrefab, chordKeyContainer);
            btn.name = "ChordKey_" + chordKeys[i];
            btn.transform.localPosition = new Vector3(0, 0, i * spacing);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = chordKeys[i];

            KeyButton keyButton = btn.AddComponent<KeyButton>();
            keyButton.value = chordKeys[i];
            keyButton.OnButtonPressed = OnChordKeySelected;
        }
    }

    public void OnInstrumentSelected(string selectedValue)
    {
        // Expect selectedValue to be a string representing an integer.
        if (int.TryParse(selectedValue, out int index))
        {
            currentInstrumentIndex = index;
            Debug.Log("Instrument selected: " + instrumentNames[currentInstrumentIndex]);
            UpdatePreviewDisplay();
        }
        else
        {
            Debug.LogWarning("Failed to parse instrument index from " + selectedValue);
        }
    }

    // Callback when a chord type is selected.
    public void OnChordTypeSelected(string selectedValue)
    {
        currentChordType = selectedValue;
        Debug.Log("Chord type selected: " + currentChordType);
        UpdatePreviewDisplay();
    }

    // Callback when a chord key is selected.
    public void OnChordKeySelected(string selectedValue)
    {
        currentKey = selectedValue;
        Debug.Log("Chord key selected: " + currentKey);
        UpdatePreviewDisplay();
    }

    void UpdatePreviewDisplay()
    {
        if (previewChordBlock != null)
        {
            // Retrieve the PreviewChordBlock component and update the chord values.
            PreviewChordBlock pcb = previewChordBlock.GetComponent<PreviewChordBlock>();
            if (pcb != null)
            {
                // Update the preview block’s chord data using SetChord().
                // For example, set the chord key, chord type, and default instrument index.
                pcb.SetChord(currentKey, currentChordType, currentInstrumentIndex);
            }
            else
            {
                // Fallback: update the text on the preview block.
                TMP_Text label = previewChordBlock.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = currentKey + " " + currentChordType + " " + currentInstrumentIndex;
            }
        }
    }
}

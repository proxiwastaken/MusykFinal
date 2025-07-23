using UnityEngine;

public class SequencerResizerButtons : MonoBehaviour
{
    public VRSequencer sequencer; // Assign this via the Inspector

    // Increase rows by 1
    public void IncreaseRows()
    {
        int newRows = sequencer.rows + 1;
        sequencer.ResizeGrid(newRows, sequencer.columns);
    }

    // Decrease rows by 1 (minimum 1 row)
    public void DecreaseRows()
    {
        int newRows = Mathf.Max(1, sequencer.rows - 1);
        sequencer.ResizeGrid(newRows, sequencer.columns);
    }

    // Increase columns by 1
    public void IncreaseColumns()
    {
        int newColumns = sequencer.columns + 1;
        sequencer.ResizeGrid(sequencer.rows, newColumns);
    }

    // Decrease columns by 1 (minimum 1 column)
    public void DecreaseColumns()
    {
        int newColumns = Mathf.Max(1, sequencer.columns - 1);
        sequencer.ResizeGrid(sequencer.rows, newColumns);
    }
}

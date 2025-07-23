using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewInstrumentBank", menuName = "Audio/Instrument Bank")]
public class InstrumentBank : ScriptableObject
{
    [System.Serializable]
    public class InstrumentEntry
    {
        public string instrumentName;
        [Tooltip("12 AudioClips for one octave (e.g., C, C#, D, …, B)")]
        public AudioClip[] notes;  // Expect exactly 12 notes per instrument.
    }

    [Header("Instruments")]
    [Tooltip("List of instruments, each with 12 note samples.")]
    public List<InstrumentEntry> instruments = new List<InstrumentEntry>();

    /// <summary>
    /// Returns the AudioClip for the given instrument and note index.
    /// </summary>
    public AudioClip GetClip(int instrumentIndex, int noteIndex)
    {
        if (instrumentIndex < 0 || instrumentIndex >= instruments.Count)
        {
            Debug.LogWarning("Invalid instrument index.");
            return null;
        }
        InstrumentEntry entry = instruments[instrumentIndex];
        if (entry.notes == null || noteIndex < 0 || noteIndex >= entry.notes.Length)
        {
            Debug.LogWarning("Invalid note index for instrument " + entry.instrumentName);
            return null;
        }
        return entry.notes[noteIndex];
    }
}

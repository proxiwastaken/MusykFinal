using UnityEngine;
using System;

public class GlobalSequencerClock : MonoBehaviour
{
    // Singleton instance for easy access.
    public static GlobalSequencerClock Instance;

    [Header("Clock Settings")]
    public float _bpm = 120f;
    public float bpm
    {
        get => _bpm;
        set
        {
            _bpm = Mathf.Max(1f, value);                  // clamp to a minimum
            RecalculateInterval();
            // schedule the next beat relative to now, so change is immediate
            nextBeatTime = AudioSettings.dspTime + beatInterval;
        }
    }
    public int stepsPerBeat = 1;  // For example, if each beat is a step.

    // Event that gets fired on each beat.
    public event Action<double> OnBeat;

    private double nextBeatTime;
    private double beatInterval;

    void Awake()
    {
        Debug.Log("GlobalSequencerClock Awake. Instance set.");
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        RecalculateInterval();
        nextBeatTime = AudioSettings.dspTime + beatInterval;
    }

    void Update()
    {
        double dspTime = AudioSettings.dspTime;
        if (dspTime >= nextBeatTime)
        {
            double scheduledTime = nextBeatTime;

            OnBeat?.Invoke(scheduledTime);
            nextBeatTime += beatInterval;
        }
    }

    void RecalculateInterval()
    {
        beatInterval = 60.0 / (_bpm * stepsPerBeat);
    }
}

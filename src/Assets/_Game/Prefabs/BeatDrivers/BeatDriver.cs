using System;
using UnityEngine;

public class BeatDriver : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource source;      // assign in Inspector; disable Play On Awake
    [Header("Beat Grid")]
    public float bpm = 100f;        // your track’s BPM
    [Tooltip("Time in seconds from song start to the first beat (if intro/silence).")]
    public double firstBeatOffset = 0.0;
    [Tooltip("1=quarter notes, 2=eighths, 4=sixteenths, etc.")]
    public int subdivision = 1;

    public event Action OnTick;     // subscribe your gameplay here

    public double SongStartDsp { get; private set; }            // when the song actually starts (DSP clock)

    public double NextTickDsp { get; private set; }            // next tick time on the DSP clock

    public int CurrentTick { get; private set; }

    public double SPB { get; private set; }                     // seconds per (subdivided) beat

    private void Awake()
    {
        SPB = 60.0 / bpm / subdivision;
    }

    private void Start()
    {
        // Schedule start slightly in the future for stable timing
        SongStartDsp = AudioSettings.dspTime + 0.15;
        source.PlayScheduled(SongStartDsp);

        NextTickDsp = SongStartDsp + firstBeatOffset + SPB; // first tick after start
    }

    private void Update()
    {
        if (!source.isPlaying)
        {
            return;
        }

        double dsp = AudioSettings.dspTime;

        // Catch up if more than one tick elapsed this frame
        while (dsp >= NextTickDsp)
        {
            CurrentTick++;
            OnTick?.Invoke();
            NextTickDsp += SPB;
        }
    }

    // If you need to support pause/resume, adjust nextTickDsp by the pause delta.
    public void Pause()
    {
        source.Pause();
    }

    public void Resume()
    {
        // Re-align nextTickDsp to the current song position
        source.UnPause();
        double songPos = AudioSettings.dspTime - SongStartDsp - firstBeatOffset;
        if (songPos < 0)
        {
            songPos = 0;
        }

        double ticksPassed = Math.Floor(songPos / SPB);
        NextTickDsp = SongStartDsp + firstBeatOffset + ((ticksPassed + 1) * SPB);
    }

    public bool IsPlaying()
    {
        return source.isPlaying;
    }
}

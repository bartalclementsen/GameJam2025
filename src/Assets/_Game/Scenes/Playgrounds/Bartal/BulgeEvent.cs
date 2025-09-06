// BulgeEvent.cs
using System;
using UnityEngine;

[Serializable]
public class BulgeEvent
{
    [Header("Where (musical)")]
    public int bar;                    // 0-based
    [Range(0, 15)] public int beat;    // 0..beatsPerBar-1

    [Header("Which line")]
    public bool appliesToAllLines = false;
    public int staffIndex = 0;         // ignored if appliesToAllLines

    [Header("Shape")]
    public float strength = -0.45f;    // + up bulge, - down sag (world units)
    public float widthBeats = 0.8f;    // how wide the bump is, in beats
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [HideInInspector] public float centerX; // filled by SongGrid

    public BulgeEvent Clone()
    {
        return new BulgeEvent
        {
            bar = bar,
            beat = beat,
            appliesToAllLines = appliesToAllLines,
            staffIndex = staffIndex,
            strength = strength,
            widthBeats = widthBeats,
            falloff = new AnimationCurve(falloff.keys),
            centerX = centerX
        };
    }
}

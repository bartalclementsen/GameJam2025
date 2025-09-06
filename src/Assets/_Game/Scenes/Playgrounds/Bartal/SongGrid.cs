// SongGrid.cs
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SongGrid : MonoBehaviour
{
    [Header("Structure")]
    public int bars = 16;
    public int beatsPerBar = 4;
    public float unitsPerBeat = 2f;         // horizontal size of one beat in world units
    public float staffHeight = 3f;          // total vertical height of the 5 lines
    [Range(2, 9)] public int staffLines = 5; // 5 by default

    [Header("Look")]
    public Color lineColor = new(0.1f, 0.9f, 1f);
    [Range(0.01f, 0.25f)] public float coreWidth = 0.06f;
    [Range(0.02f, 0.6f)] public float glowWidth = 0.28f;
    [Range(0f, 1f)] public float glowAlpha = 0.4f;

    [Header("Wriggle")]
    [Range(0f, 0.3f)] public float wriggleAmplitude = 0.07f;
    [Range(0f, 10f)] public float wriggleFrequency = 2.0f; // cycles per world unit
    [Range(0f, 5f)] public float wriggleSpeed = 0.6f;     // time multiplier

    [Header("Resolution & Holders")]
    [Range(64, 4096)] public int lineResolution = 512; // points per line
    public Transform staffRoot; // parent for lines and bars

    [Header("Bars/Beats")]
    public bool drawBeatTicks = true;
    public float barLineWidth = 0.05f;
    public float beatLineWidth = 0.02f;
    public Color barColor = new(1, 1, 1, 0.6f);
    public Color beatColor = new(1, 1, 1, 0.25f);

    [Header("Bulges/Sags")]
    public List<BulgeEvent> bulges = new(); // author in Inspector

    private readonly List<WigglyLine> _lines = new();
    private Transform _barsRoot;

    private float TotalWidth => bars * beatsPerBar * unitsPerBeat;

    public float XForBeat(int bar, int beat)
    {
        return ((bar * beatsPerBar) + beat) * unitsPerBeat;
    }

    private void OnEnable()
    {
        Build();
    }

    private void OnValidate()
    {
        Build();
    }

    private void Update()
    {
        UpdateLines(Time.time);
    }

    private void Build()

    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Destroy(this.transform.GetChild(i).gameObject);
        }

        staffRoot = new GameObject("StaffRoot").transform;
        staffRoot.SetParent(transform, false);

        // Clear existing
        //for (int i = staffRoot.childCount - 1; i >= 0; i--)
        //{
        //    Destroy(staffRoot.GetChild(i).gameObject);
        //}

        _lines.Clear();

        // Make 5 (or N) horizontal wriggly lines
        float dy = staffLines > 1 ? staffHeight / (staffLines - 1) : 0f;
        float y0 = -staffHeight * 0.5f;
        for (int i = 0; i < staffLines; i++)
        {
            float y = y0 + (dy * i);
            var go = new GameObject($"Line_{i}");
            go.transform.SetParent(staffRoot, false);
            WigglyLine wl = go.AddComponent<WigglyLine>();
            wl.length = TotalWidth;
            wl.yBase = y;
            wl.resolution = lineResolution;

            // appearance
            wl.color = lineColor;
            wl.coreWidth = coreWidth;
            wl.glowWidth = glowWidth;
            wl.glowAlpha = glowAlpha;

            // motion
            wl.wriggleAmplitude = wriggleAmplitude;
            wl.wriggleFrequency = wriggleFrequency;
            wl.wriggleSpeed = wriggleSpeed;

            // pass bulges that apply to this staff index (or to all)
            wl.bulges.Clear();
            foreach (BulgeEvent b in bulges)
            {
                if (b.appliesToAllLines || b.staffIndex == i)
                {
                    // Convert beat->x and copy
                    BulgeEvent copy = b.Clone();
                    copy.centerX = XForBeat(b.bar, b.beat); // precalc world X
                    wl.bulges.Add(copy);
                }
            }

            wl.Build(); // allocate renderers
            _lines.Add(wl);
        }

        // Bars & beats
        if (_barsRoot != null)
        {
            DestroyImmediate(_barsRoot.gameObject);
        }

        _barsRoot = new GameObject("BarsBeats").transform;
        _barsRoot.SetParent(staffRoot, false);

        float yMin = y0 - (dy * 0.7f);
        float yMax = y0 + (dy * (staffLines - 1)) + (dy * 0.7f);

        // Bar lines
        for (int bar = 0; bar <= bars; bar++)
        {
            float x = XForBeat(bar, 0);
            DrawVertical(_barsRoot, $"Bar_{bar}", x, yMin, yMax, barLineWidth, barColor);
            if (!drawBeatTicks || bar == bars)
            {
                continue;
            }

            // Beat ticks inside bar
            for (int beat = 1; beat < beatsPerBar; beat++)
            {
                float xb = XForBeat(bar, beat);
                DrawVertical(_barsRoot, $"Beat_{bar}_{beat}", xb, yMin, yMax, beatLineWidth, beatColor);
            }
        }
    }

    private void DrawVertical(Transform parent, string name, float x, float yMin, float yMax, float width, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPositions(new Vector3[] { new(x, yMin, 0), new(x, yMax, 0) });
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = lr.endWidth = width;
        lr.startColor = lr.endColor = color;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 2;
        lr.textureMode = LineTextureMode.Stretch;
        lr.sortingOrder = 5;
    }

    private void UpdateLines(float t)
    {
        foreach (WigglyLine l in _lines)
        {
            l.UpdateLine(t);
        }
    }
}

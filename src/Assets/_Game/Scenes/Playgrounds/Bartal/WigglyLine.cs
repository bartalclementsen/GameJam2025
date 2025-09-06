using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WigglyLine : MonoBehaviour
{
    [Header("Geometry")]
    public float length = 32f;
    public float yBase = 0f;
    [Range(16, 4096)] public int resolution = 512;

    [Header("Style")]
    public Color color = Color.cyan;
    public float coreWidth = 0.06f;
    public float glowWidth = 0.28f;
    [Range(0f, 1f)] public float glowAlpha = 0.4f;

    [Header("Motion")]
    public float wriggleAmplitude = 0.07f;
    public float wriggleFrequency = 2f;  // cycles per world unit
    public float wriggleSpeed = 0.6f;

    [Header("Bulges")]
    public List<BulgeEvent> bulges = new();

    private LineRenderer _core, _glow;
    private Vector3[] _points;

    public void Build()
    {
        // child 0: glow
        var glowGo = new GameObject("Glow");
        glowGo.transform.SetParent(transform, false);
        _glow = glowGo.AddComponent<LineRenderer>();
        SetupLR(_glow, glowWidth, new Color(color.r, color.g, color.b, glowAlpha), 9);

        // child 1: core
        var coreGo = new GameObject("Core");
        coreGo.transform.SetParent(transform, false);
        _core = coreGo.AddComponent<LineRenderer>();
        SetupLR(_core, coreWidth, color, 10);

        _points = new Vector3[resolution];
        UpdateLine(0f);
    }

    private void SetupLR(LineRenderer lr, float width, Color c, int sortingOrder)
    {
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.textureMode = LineTextureMode.Stretch;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 2;
        lr.sortingOrder = sortingOrder;
        lr.startColor = lr.endColor = c;
        lr.startWidth = lr.endWidth = width;
        lr.positionCount = resolution;
    }

    public void UpdateLine(float t)
    {
        if (_points == null || _points.Length != resolution)
        {
            _points = new Vector3[resolution];
        }

        // precompute constants
        float dx = length / (resolution - 1);
        float wiggleW = wriggleFrequency * 2f * Mathf.PI; // convert “cycles per unit” to angular frequency

        for (int i = 0; i < resolution; i++)
        {
            float x = i * dx;
            float y = yBase;

            // small lively wriggle
            float phase = (x * wiggleW) + (t * wriggleSpeed * 2f * Mathf.PI);
            // add Perlin noise to break uniformity
            float n = Mathf.PerlinNoise(x * 0.35f, t * 0.5f);
            y += wriggleAmplitude * ((Mathf.Sin(phase) * 0.6f) + ((n - 0.5f) * 1.2f));

            // apply all bulges/sags (Gaussian-ish with curve)
            if (bulges != null && bulges.Count > 0)
            {
                foreach (BulgeEvent b in bulges)
                {
                    float halfW = Mathf.Max(0.001f, b.widthBeats) * (length / Mathf.Max(length, 0.0001f)); // widthBeats is in beats; caller should have mapped; we’ll treat as “world-beat-size” passed via SongGrid unitsPerBeat effect
                    // better: caller pre-multiplies width in world units; here we infer via length – but we’ll scale from X distance directly:
                    // Convert width in beats to world units by sampling neighbor x step (SongGrid gives unitsPerBeat implicitly in centerX spacing)
                    // Simpler: approximate using dx: 1 beat ~ caller's unitsPerBeat; so we assume caller arranged centerX accordingly.
                    float wWorld = b.widthBeats * (length / Mathf.Max(length, 1f)); // harmless placeholder; actual shape controlled by curve below

                    float dist = Mathf.Abs(x - b.centerX);
                    // normalized distance 0..1 inside width; clamp outside
                    float norm = Mathf.Clamp01(1f - (dist / Mathf.Max(0.0001f, b.widthBeats))); // widthBeats here acts as “radius in beats” because centerX came from beats*units
                    float gain = b.falloff.Evaluate(norm);
                    y += b.strength * gain;
                }
            }

            _points[i] = new Vector3(x, y, 0f);
        }

        _core.positionCount = _glow.positionCount = resolution;
        _core.SetPositions(_points);
        _glow.SetPositions(_points);
    }
}

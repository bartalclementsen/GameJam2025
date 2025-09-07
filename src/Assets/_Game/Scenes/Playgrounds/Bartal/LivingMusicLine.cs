using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LivingMusicLine : MonoBehaviour
{
    [Header("Geometry")]
    public float length = 20f;          // world units left->right
    public float laneY = 0f;            // base horizontal position
    public int segments = 256;          // resolution of the line
    public float thickness = 0.08f;

    [Header("Wriggle (feel)")]
    public float wriggleAmplitude = 0.15f;    // vertical
    public float wriggleTimeFreq = 2.0f;      // Hz over time
    public float wriggleSpatialFreq = 1.5f;   // waves across length
    public float noiseAmplitude = 0.05f;      // extra jitter
    public float noiseScroll = 0.7f;

    [Header("Glow/Color")]
    public Color baseColor = new(0, 1, 1, 1);   // cyan
    [ColorUsage(true, true)] public Color baseEmission = new(0, 4, 4); // HDR for bloom
    [ColorUsage(true, true)] public Color highlightEmission = new(0, 8, 8);
    public float highlightLerpSpeed = 8f;

    [Header("Sag/Bulge")]
    public AnimationCurve sagEnvelope = AnimationCurve.EaseInOut(0, 1, 1, 0); // temporal decay
    public float healSpeed = 3f; // how quickly a fixed sag returns to flat

    [Header("Scrubber/Dot")]
    public Transform scrubber; // moves left->right
    public float dotY;         // the player-controlled vertical dot position
    public float hitXRadius = 0.15f;  // how close in X the dot must be
    public float hitYRadius = 0.15f;  // how close in Y the dot must be

    private LineRenderer lr;
    private Material matInstance;
    private Vector3[] pts;

    [Serializable]
    public class SagEvent
    {
        public float centerX;     // along the line [0, length]
        public float amplitude;   // + bulge, - sag
        public float width = 0.6f;     // Gaussian width (world units)
        public float startTime;   // seconds (audio time)
        public float duration = 0.7f;  // seconds
        public bool fixedByPlayer = false;
        public float fixProgress = 0f; // 0..1 easing to healed
    }

    public List<SagEvent> sags = new();

    private float t; // local clock; you can sync this to music dsp time if desired
    private float emissionBlend = 0f; // 0 base, 1 highlighted

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        //lr.textureMode = LineTextureMode.Stretch;
        //lr.widthCurve = AnimationCurve.Constant(0, 1, thickness);

        // Use a unique material instance so emission can animate per-line:
        matInstance = Instantiate(lr.sharedMaterial);
        lr.material = matInstance;

        pts = Enumerable.Range(0, segments).Select(i => new Vector3(i, 0, 0)).ToArray();
        lr.SetPositions(pts);
    }

    private void Update()
    {
        t += Time.deltaTime;

        // 1) Build the wriggly line
        for (int i = 0; i < segments; i++)
        {
            float u = i / (segments - 1f);
            float x = u * length;

            float y = laneY;

            // Wriggle (sine + scrolling noise)
            float sine = Mathf.Sin((x * wriggleSpatialFreq * Mathf.PI * 2f) + (t * wriggleTimeFreq * Mathf.PI * 2f));
            float noise = (Mathf.PerlinNoise(x * 0.8f, t * noiseScroll) - 0.5f) * 2f;
            y += (wriggleAmplitude * sine) + (noiseAmplitude * noise);

            // Sag/Bulge contributions
            float sagY = 0f;
            for (int s = 0; s < sags.Count; s++)
            {
                SagEvent ev = sags[s];
                float elapsed = Mathf.Clamp01((Time.time - ev.startTime) / Mathf.Max(0.0001f, ev.duration));
                float env = sagEnvelope.Evaluate(elapsed);

                // Heal if fixed
                if (ev.fixedByPlayer)
                {
                    ev.fixProgress = Mathf.MoveTowards(ev.fixProgress, 1f, Time.deltaTime * healSpeed);
                }

                float effectiveAmp = ev.amplitude * env * (1f - ev.fixProgress);
                if (Mathf.Approximately(effectiveAmp, 0f))
                {
                    continue;
                }

                // Gaussian bump centered at centerX
                float dx = x - ev.centerX;
                float g = Mathf.Exp(-(dx * dx) / (2f * ev.width * ev.width));
                sagY += effectiveAmp * g;
            }
            y += sagY;

            pts[i] = new Vector3(x, y, 0f);
        }

        lr.SetPositions(pts);

        // 2) Highlight if the dot is “on” the line at the scrubber x
        bool onLine = false;
        if (scrubber != null)
        {
            float sx = Mathf.Clamp(scrubber.position.x, 0f, length);
            float lineYAtScrubber = SampleYAtX(sx);
            float dx = Mathf.Abs(scrubber.position.x - sx); // usually 0 if scrubber constrained to [0,length]
            float dy = Mathf.Abs(dotY - lineYAtScrubber);

            onLine = (dx <= hitXRadius) && (dy <= hitYRadius);

            if (onLine)
            {
                // Mark any nearby sag as fixed
                foreach (SagEvent ev in sags)
                {
                    if (Mathf.Abs(ev.centerX - sx) <= hitXRadius * 1.5f)
                    {
                        ev.fixedByPlayer = true;
                    }
                }
            }
        }

        // 3) Animate glow when highlighted
        emissionBlend = Mathf.MoveTowards(emissionBlend, onLine ? 1f : 0f, Time.deltaTime * highlightLerpSpeed);
        Color currentEmission = Color.Lerp(baseEmission, highlightEmission, emissionBlend);
        matInstance.SetColor("_BaseColor", baseColor);      // URP Unlit uses _BaseColor
        matInstance.SetColor("_EmissionColor", currentEmission);
    }

    private float SampleYAtX(float x)
    {
        // Fast lookup via segment index
        float u = Mathf.Clamp01(x / length);
        float fIndex = u * (segments - 1f);
        int i0 = Mathf.Clamp(Mathf.FloorToInt(fIndex), 0, segments - 1);
        int i1 = Mathf.Min(i0 + 1, segments - 1);
        float a = fIndex - i0;
        return Mathf.Lerp(pts[i0].y, pts[i1].y, a);
    }

    // Call this when scheduling a sag/bulge at a beat
    public void AddSagBulge(float centerX, float amplitude, float width, float duration)
    {
        sags.Add(new SagEvent
        {
            centerX = Mathf.Clamp(centerX, 0f, length),
            amplitude = amplitude,   // negative for sag, positive for bulge
            width = Mathf.Max(0.05f, width),
            startTime = Time.time,
            duration = Mathf.Max(0.05f, duration)
        });
    }

    // Utility: convert music time (sec) to X if your scrubber follows audio
    public float TimeToX(float seconds, float songDuration)
    {
        if (songDuration <= 0f)
        {
            return 0f;
        }

        return Mathf.Clamp01(seconds / songDuration) * length;
    }
}
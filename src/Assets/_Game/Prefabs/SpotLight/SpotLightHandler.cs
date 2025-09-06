using Core.Loggers;
using UnityEngine;

public class SpotLightHandler : MonoBehaviour
{
    [SerializeField]
    private float _triangleMaxAngle = 135;

    [SerializeField]
    private float _triangleMinAngle = 105;

    [SerializeField]
    private int _triangleRotationCycleTime = 1000;

    [SerializeField]
    private float _triangleMaxXScale = 4;

    [SerializeField]
    private float _triangleMinXScale = 2;

    [SerializeField]
    private int _triangleScaleCycleTime = 1000;

    private Core.Loggers.ILogger _logger;

    private SpriteRenderer _spriteRenderer;


    private float _angleLerpFrameTimeCounter = 0f;
    private bool _incrementingAngle = true;
    private float _startAngleDeg = 0f;

    private float _scaleLerpFrameTimeCounter = 0f;
    private bool _incrementingScale = true;
    private float _startScale = 0f;

    void Start()
    {
        _logger = Game.Container.Resolve<ILoggerFactory>().Create(this);
        _logger.Log("SpotLightHandler started");
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        // Initialize start angle from current local Z rotation (degrees)
        _startAngleDeg = _spriteRenderer.transform.localEulerAngles.z;
        _startScale = _spriteRenderer.transform.localScale.x;
    }

    void FixedUpdate()
    {
        ApplyRotation();
        ApplyScale();
    }

    private void ApplyRotation()
    {
        _angleLerpFrameTimeCounter += Time.deltaTime;
        float lerpTimeInSeconds = _triangleRotationCycleTime / 1000f;
        float interpolationFraction = Mathf.Clamp01(_angleLerpFrameTimeCounter / lerpTimeInSeconds);

        float targetAngleDeg = _incrementingAngle ? _triangleMaxAngle : _triangleMinAngle;

        // Interpolate in degrees (handles wrap-around correctly)
        float newZDeg = Mathf.LerpAngle(_startAngleDeg, targetAngleDeg, interpolationFraction);

        // Apply rotation preserving X/Y
        Vector3 localEulerAngles = _spriteRenderer.transform.localEulerAngles;
        localEulerAngles.z = newZDeg;

        _spriteRenderer.transform.localEulerAngles = localEulerAngles;

        // When cycle complete, reset counter and flip direction, set start angle for next cycle
        if (_angleLerpFrameTimeCounter >= lerpTimeInSeconds)
        {
            _angleLerpFrameTimeCounter = 0f;

            // ensure exact start for next cycle
            _startAngleDeg = targetAngleDeg;
            _incrementingAngle = !_incrementingAngle;
        }
    }

    private void ApplyScale()
    {
        _scaleLerpFrameTimeCounter += Time.deltaTime;
        float lerpTimeInSeconds = _triangleScaleCycleTime / 1000f;
        float interpolationFraction = Mathf.Clamp01(_scaleLerpFrameTimeCounter / lerpTimeInSeconds);

        float targetScale = _incrementingScale ? _triangleMaxXScale : _triangleMinXScale;

        // Interpolate in degrees (handles wrap-around correctly)
        float newXScale = Mathf.Lerp(_startScale, targetScale, interpolationFraction);

        // Apply rotation preserving X/Y
        Vector3 localScale = _spriteRenderer.transform.localScale;
        localScale.x = newXScale;

        _spriteRenderer.transform.localScale = localScale;

        // When cycle complete, reset counter and flip direction, set start angle for next cycle
        if (_scaleLerpFrameTimeCounter >= lerpTimeInSeconds)
        {
            _scaleLerpFrameTimeCounter = 0f;

            // ensure exact start for next cycle
            _startScale = targetScale;
            _incrementingScale = !_incrementingScale;
        }
    }
}
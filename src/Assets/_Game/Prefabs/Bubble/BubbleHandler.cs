using UnityEngine;

public class BubbleHandler : MonoBehaviour
{
    [Header("Circle Scale Animation Settings")]
    [SerializeField]
    private float _circleMaxScale = 4;

    [SerializeField]
    private float _circleMinScale = 2;

    [SerializeField]
    private int _circleScaleCycleTime = 1000;

    [SerializeField]
    private Color _color;

    [SerializeField]
    [Range(0f, 1f)]
    private float _maxAlpha;

    [SerializeField]
    [Range(0f, 1f)]
    private float _minAlpha;

    [SerializeField]
    [Range(0.1f, 5f)]
    private float _alphaChangeSpeed = 1f;

    private SpriteRenderer _spriteRenderer;

    private float _scaleLerpFrameTimeCounter = 0f;
    private bool _incrementingScale = true;
    private float _startScale = 0f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // Apply initial
        float initialScale = Random.Range(_circleMinScale, _circleMaxScale);
        Vector3 localScale = _spriteRenderer.transform.localScale;
        localScale.x = initialScale;
        localScale.y = initialScale;
        _spriteRenderer.transform.localScale = localScale;

        _color.a = Random.Range(_minAlpha, _maxAlpha);

        // We don't care for y scale, as we want to scale the circle same in x- and y-axis
        _startScale = _spriteRenderer.transform.localScale.x;
        _spriteRenderer.color = _color;
    }

    // Update is called once per frame
    private bool alphaUpDirection = true;

    void Update()
    {
        ApplyScale();
        var alpha = _color.a;
        if(alphaUpDirection)
        {
            alpha += Time.deltaTime * _alphaChangeSpeed;
            if(alpha >= _maxAlpha)
            {
                alpha = _maxAlpha;
                alphaUpDirection = false;
            }
        }
        else
        {
            alpha -= Time.deltaTime * _alphaChangeSpeed;
            if(alpha <= _minAlpha)
            {
                alpha = _minAlpha;
                alphaUpDirection = true;
            }
        }

        //_color.a = _maxAlpha - Mathf.PingPong(Time.time * _alphaChangeSpeed, _maxAlpha - _minAlpha);
        _color.a = alpha;
        _spriteRenderer.color = _color;
    }

    private void ApplyScale()
    {
        _scaleLerpFrameTimeCounter += Time.deltaTime;
        float lerpTimeInSeconds = _circleScaleCycleTime / 1000f;
        float interpolationFraction = Mathf.Clamp01(_scaleLerpFrameTimeCounter / lerpTimeInSeconds);

        float targetScale = _incrementingScale ? _circleMaxScale : _circleMinScale;

        //  Linearly interpolate between start and target scale
        float newScale = Mathf.Lerp(_startScale, targetScale, interpolationFraction);

        // Apply X scale preserving Y/Z
        Vector3 localScale = _spriteRenderer.transform.localScale;
        localScale.x = newScale;
        localScale.y = newScale;

        _spriteRenderer.transform.localScale = localScale;

        // When cycle complete, reset counter and flip direction, set start scale for next cycle
        if (_scaleLerpFrameTimeCounter >= lerpTimeInSeconds)
        {
            _scaleLerpFrameTimeCounter = 0f;

            // ensure exact start for next cycle
            _startScale = targetScale;
            _incrementingScale = !_incrementingScale;
        }
    }
}

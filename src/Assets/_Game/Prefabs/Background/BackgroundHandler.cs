using System;
using Core.Loggers;
using UnityEngine;
using UnityEngine.Assertions;

public class BackgroundHandler : MonoBehaviour
{
    [SerializeField]
    private int _lerpTime;

    [SerializeField]
    private Color[] _colors;

    private SpriteRenderer _spriteRenderer;

    private Core.Loggers.ILogger _logger;

    private DateTime _lastUpdateTime = DateTime.MinValue;

    private Color _baseColor;

    private Color _targetColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _logger = Game.Container.Resolve<ILoggerFactory>().Create(this);
        Assert.IsTrue(_colors.Length > 1, "BackgroundHandler requires at least two colors to function.");
        _baseColor = _colors[0];
        _targetColor = _colors[1];
        _lastUpdateTime = DateTime.Now;
    }

    // Fixed 60 frames per second
    private void FixedUpdate()
    {
        // Change color once lerp has reached max
        DateTime nextResetCycle = _lastUpdateTime.AddMilliseconds(_lerpTime);
        if (nextResetCycle < DateTime.Now)
        {
            _lastUpdateTime = DateTime.Now;
            int currentColorIndex = FindColorIndex(_targetColor);
            _baseColor = _colors[currentColorIndex];
            _logger.Log($"Changed base color to index {currentColorIndex}");

            if (currentColorIndex == _colors.Length - 1)
            {
                _targetColor = _colors[0];
                _logger.Log("Changed target color to index 0");
            }
            else
            {
                _targetColor = _colors[currentColorIndex + 1];
                _logger.Log($"Changed target color to index {currentColorIndex + 1}");
            }
        }

        float lerpFactor = ((float)(DateTime.Now - _lastUpdateTime).Milliseconds) / _lerpTime;
        Color interpolatedColor = Color.Lerp(_baseColor, _targetColor, lerpFactor);
        _spriteRenderer.color = interpolatedColor;
    }

    private int FindColorIndex(Color color)
    {
        for (int i = 0; i < _colors.Length; i++)
        {
            if (_colors[i] == color)
            {
                return i;
            }
        }

        return -1;
    }
}

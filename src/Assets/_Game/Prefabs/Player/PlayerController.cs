using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Pulse Visual")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _tickSprite;
    [SerializeField] private SpriteRenderer _renderer;

    private Vector3 originalScale;
    private Coroutine pulseRoutine;

    private int index = 0;

    void Awake()
    {
        originalScale = _renderer.gameObject.transform.localScale;
    }

    public void Tick()
    {
        if(index % 2 == 0)
        { 
            Pulse();
        }

        index++;
    }

    public void Pulse()
    {
        // Stop ongoing pulse if called again quickly
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    private System.Collections.IEnumerator PulseRoutine()
    {
        float duration = 0.10f; // 150ms
        float halfDuration = duration / 2f;
        float time = 0f;
        float scaleFactor = 2f; // how big the pulse goes

        // Scale up
        while (time < halfDuration)
        {
            float t = time / halfDuration;
            _renderer.gameObject.transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleFactor, t);
            time += Time.deltaTime;
            yield return null;
        }

        // Scale down
        time = 0f;
        while (time < halfDuration)
        {
            float t = time / halfDuration;
            _renderer.gameObject.transform.localScale = Vector3.Lerp(originalScale * scaleFactor, originalScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        _renderer.gameObject.transform.localScale = originalScale; // ensure reset
        pulseRoutine = null;
    }
}

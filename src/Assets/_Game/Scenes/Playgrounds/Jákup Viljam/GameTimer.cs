using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Jákup_Viljam
{
    public class GameTimer : MonoBehaviour
    {
        [SerializeField]
        private float _startTime;
        [SerializeField]
        private TextMeshProUGUI _timerText;
        [SerializeField]
        private Color _startColor = Color.white;
        [SerializeField]
        private Color _endColor = Color.red;
        [SerializeField]
        private float _colorTransitionSpeed = 2f;

        private float _currentTime;

        public void Start()
        {
            _currentTime = _startTime;

            SetText();
        }

        public void Update()
        {
            if (_currentTime > 0)
            {
                _currentTime -= Time.deltaTime;
                if (_currentTime < 0)
                {
                    _currentTime = 0;
                }
            }

            SetText();

            SetColor();
        }

        private void SetText()
        {
            int minutes = Mathf.FloorToInt(_currentTime / 60);
            int seconds = Mathf.FloorToInt(_currentTime % 60);

            _timerText.text = $"{minutes:00}:{seconds:00}";
        }

        private void SetColor()
        {
            float t = (Mathf.Sin(Time.time * _colorTransitionSpeed) + 1f) / 2f;
            _timerText.color = Color.Lerp(_startColor, _endColor, t);
        }
    }
}

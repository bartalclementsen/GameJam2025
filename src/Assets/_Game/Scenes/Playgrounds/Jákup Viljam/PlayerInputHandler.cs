using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Jákup_Viljam
{
    public class PlayerInputHandler : MonoBehaviour
    {
        public float PerfectWindowMs = 50f;
        public float GoodWindowMs = 100f;

        [SerializeField]
        private RhythmHandler _rhythmHandler;
        [SerializeField]
        private PlayerHandler _playerHandler;

        private Core.Loggers.ILogger _logger;

        void Start()
        {
            _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);
        }

        void Update()
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                HandleInput(+1);

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                HandleInput(-1);
        }

        void HandleInput(int direction)
        {
            float now = Time.time * 1000f;
            float diff = Mathf.Abs(now - _rhythmHandler.NextTickTimeMs()); // how far from expected tick

            _playerHandler.TryMoveVertical(direction);

            int score = 0;
            if (diff <= PerfectWindowMs)
            {
                score = 100;
            }
            else if (diff <= GoodWindowMs)
            {
                score = 50;
            }
            else
            {
                score = 10;
            }

            _logger?.Log($"Input {direction}, Accuracy {diff:F0}ms, Score {score}");
        }
    }

}

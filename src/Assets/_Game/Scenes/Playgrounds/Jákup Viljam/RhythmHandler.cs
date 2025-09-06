using UnityEngine;

namespace Jákup_Viljam
{
    public class RhythmHandler : MonoBehaviour
    {
        public int BPM = 100;
        public int Subdivisions = 8;
        public int CurrentTick { get; private set; }

        [SerializeField]
        private JVDGameHandler _gameHandler;

        private float _msPerTick;
        private float _nextTickTime;

        private Core.Loggers.ILogger _logger;

        private void Start()
        {
            _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);

            _msPerTick = 60000f / BPM / (Subdivisions / 4f);
            _nextTickTime = (Time.time * 1000f) + _msPerTick;
        }

        private void Update()
        {
            float now = Time.time * 1000f;

            if (now >= _nextTickTime)
            {
                OnTick();
                CurrentTick++;
                _nextTickTime += _msPerTick;
            }
        }

        private void OnTick()
        {
            _gameHandler.OnRhytmTick();
        }

        public float NextTickTimeMs()
        {
            return _nextTickTime;
        }
    }
}

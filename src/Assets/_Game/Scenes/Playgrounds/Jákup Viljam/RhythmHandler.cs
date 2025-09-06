using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jákup_Viljam
{
    public class RhythmHandler : MonoBehaviour
    {
        public int BPM = 100;
        public int Subdivisions = 8; 
        public int CurrentTick { get; private set; }

        [SerializeField]
        private PlayerHandler _playerHandler;

        private float _msPerTick;
        private float _nextTickTime;

        void Start()
        {
            _msPerTick = 60000f / BPM / (Subdivisions / 4f);
            _nextTickTime = Time.time * 1000f + _msPerTick;
        }

        void Update()
        {
            float now = Time.time * 1000f;

            if (now >= _nextTickTime)
            {
                OnTick();
                CurrentTick++;
                _nextTickTime += _msPerTick;
            }
        }

        void OnTick()
        {
            // tell player to auto-move forward one beat
            _playerHandler.AdvanceBeat();
        }

        public float NextTickTimeMs() => _nextTickTime;
    }
}

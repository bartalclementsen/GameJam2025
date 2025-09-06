using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jákup_Viljam
{
    public class PlayerHandler : MonoBehaviour
    {
        public int CurrentBar = 0;
        public int CurrentBeat = 0;
        public int CurrentLine = 2; // start middle
        public int TotalBars;
        public int BeatsPerBar;
        public int Lines;

        private Core.Loggers.ILogger _logger;

        public void Start()
        {
            _logger = Game.Container.Resolve<Core.Loggers.ILogger>();
        }

        public void Update()
        {

        }

        public void AdvanceBeat()
        {
            // move horizontally
            CurrentBeat++;
            if (CurrentBeat >= BeatsPerBar)
            {
                CurrentBeat = 0;
                CurrentBar++;
            }

            // reached end of song?
            if (CurrentBar >= TotalBars)
            {
                _logger?.Log("Song finished!");
            }
        }

        public void TryMoveVertical(int direction)
        {
            // only allow at beat tick
            CurrentLine = Mathf.Clamp(CurrentLine + direction, 0, Lines - 1);
        }
    }
}

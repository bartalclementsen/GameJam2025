using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jákup_Viljam.Models;
using UnityEngine;

namespace Jákup_Viljam
{
    public class JVDGameHandler : MonoBehaviour
    {
        public float PerfectWindowMs = 50f;
        public float GoodWindowMs = 100f;
        public int CurrentBar = 0;
        public int CurrentBeat = 0;
        public int TotalBars;
        public int BeatsPerBar;
        public int Lines;
        public int Score = 0;

        [SerializeField]
        private RhythmHandler _rhythmHandler;
        [SerializeField]
        private PlayerHandler _playerHandler;

        private bool _isGameOver;

        private Core.Loggers.ILogger _logger;

        public void Start()
        {
            _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);

            MusicGraph graph = GenerateStaticMusicGraph();
            graph.PrintStaff();
        }

        public void Update()
        {

        }

        public void OnRhytmTick()
        {
            if (_isGameOver == false)
            {
                AdvanceBeat();
            }
        }

        public void OnPlayerAction()
        {
            float now = Time.time * 1000f;
            float diff = Mathf.Abs(now - _rhythmHandler.NextTickTimeMs());

            if (diff <= PerfectWindowMs)
            {
                Score += 100;
            }
            else if (diff <= GoodWindowMs)
            {
                Score += 50;
            }
            else
            {
                Score += 10;
            }

            _logger?.Log($"Accuracy {diff:F0}ms, Score {Score}");
        }

        private void AdvanceBeat()
        {
            CurrentBeat++;
            if (CurrentBeat >= BeatsPerBar)
            {
                CurrentBeat = 0;
                CurrentBar++;
            }

            if (CurrentBar >= TotalBars)
            {
                _logger?.Log("Song finished!");
                _isGameOver = true;
            }
        }

        private MusicGraph GenerateStaticMusicGraph()
        {
            GraphStructure structure = new()
            {
                Lines = 5,
                BeatsPerBar = 8,
                Bars = 16,
                SpecialNodes = new List<MusicNode>
                {
                    //0-3
                    new(0, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(0, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(0, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(0, 4, 2, NodeType.Tangled, LineType.OnLine),
                    new(0, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(0, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(0, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(0, 6, 2, NodeType.Untangled, LineType.OnLine), 

                    new(1, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(1, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(1, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(1, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(1, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(1, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(1, 4, 3, NodeType.Tangled, LineType.OnLine), 
                    new(1, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(2, 0, 0, NodeType.Point, LineType.OnLine),
                    new(2, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(2, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(2, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(2, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(2, 4, 4, NodeType.Tangled, LineType.OnLine),
                    new(2, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(2, 6, 2, NodeType.Untangled, LineType.OnLine), 
                    new(2, 7, 2, NodeType.Point, LineType.OnLine), 

                    new(3, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(3, 0, 1, NodeType.Tangled, LineType.OnLine),
                    new(3, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(3, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(3, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(3, 4, 2, NodeType.Untangled, LineType.OnLine),
                    new(3, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(3, 7, 0, NodeType.Untangled, LineType.OnLine),


                    //4-7
                    new(4, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(4, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(4, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(4, 4, 2, NodeType.Tangled, LineType.OnLine),
                    new(4, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(4, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(4, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(4, 6, 2, NodeType.Untangled, LineType.OnLine),

                    new(5, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(5, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(5, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(5, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(5, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(5, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(5, 4, 3, NodeType.Tangled, LineType.OnLine),
                    new(5, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(6, 0, 0, NodeType.Point, LineType.OnLine),
                    new(6, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(6, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(6, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(6, 2, 0, NodeType.Untangled, LineType.OnLine),
                    new(6, 4, 4, NodeType.Tangled, LineType.OnLine),
                    new(6, 4, 1, NodeType.Untangled, LineType.OnLine),
                    new(6, 6, 2, NodeType.Untangled, LineType.OnLine),
                    new(6, 7, 2, NodeType.Point, LineType.OnLine),

                    new(7, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(7, 0, 1, NodeType.Tangled, LineType.OnLine),
                    new(7, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(7, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(7, 5, 1, NodeType.Untangled, LineType.OnLine),
                    new(7, 5, 2, NodeType.Untangled, LineType.OnLine),
                    new(7, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(7, 7, 0, NodeType.Point, LineType.OnLine),

                    //8-11
                    new(8, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(9, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 4, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 3, NodeType.Untangled, LineType.OnLine),
                    new(9, 6, 0, NodeType.Untangled, LineType.OnLine),

                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),

                    new(11, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 3, NodeType.Untangled, LineType.OnLine),

                    //12-15
                    new(8, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(8, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 5, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 0, NodeType.Untangled, LineType.OnLine),
                    new(8, 7, 1, NodeType.Untangled, LineType.OnLine),

                    new(9, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(9, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 2, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 3, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 4, 2, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(9, 5, 3, NodeType.Untangled, LineType.OnLine),
                    new(9, 6, 0, NodeType.Untangled, LineType.OnLine),

                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 1, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 0, NodeType.Untangled, LineType.OnLine),
                    new(10, 0, 3, NodeType.Untangled, LineType.OnLine),

                    new(11, 0, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 3, NodeType.Untangled, LineType.OnLine),
                    new(11, 0, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 1, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 2, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 3, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 4, 4, NodeType.Untangled, LineType.OnLine),
                    new(11, 5, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 0, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 1, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 2, NodeType.Untangled, LineType.OnLine),
                    new(11, 6, 3, NodeType.Untangled, LineType.OnLine),

                }
            };

            return GraphBuilder.BuildGraph(structure);
        }
    }
}

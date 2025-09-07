using System.Collections.Generic;
using Jákup_Viljam.Models;
using UnityEngine;

namespace Jákup_Viljam
{
    public class JVDGameHandler : MonoBehaviour
    {
        public bool IsGameOver { get; private set; }
        public float PerfectWindowMs = 50f;
        public float GoodWindowMs = 100f;
        public int Score = 0;
        public MusicNode StartNode;

        public MusicGraph MusicGraph { get; private set; }

        private Core.Loggers.ILogger _logger;

        public void Start()
        {
            _logger = Game.Container.Resolve<Core.Loggers.ILoggerFactory>().Create(this);

            MusicGraph = GenerateStaticMusicGraph();
            MusicGraph.PrintStaff();

            StartNode = MusicGraph.GetNode(0, 0, 2);
        }

        public void Update()
        {

        }

        public void OnPlayerAction(MusicNode node, float diffMs)
        {
            int score = 0;

            if (diffMs <= PerfectWindowMs)
            {
                score += 100;
            }
            else if (diffMs <= GoodWindowMs)
            {
                score += 50;
            }
            else
            {
                score += 5;
            }

            switch (node.Type)
            {
                case NodeType.Untangled:
                    score -= 100; // penalty
                    break;
                case NodeType.Point:
                    score += 50;
                    break;
                case NodeType.Tangled:
                    score += 100; // reward
                    break;
                case NodeType.Powerup:
                    // TODO: handle later
                    break;
            }

            Score += score;
            _logger?.Log($"Hit node {node.Bar}/{node.Beat}/{node.Line} → Score {score} → Total {Score}");
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
                    new(0, 0, 2, NodeType.Untangled),
                    new(0, 0, 3, NodeType.Untangled),
                    new(0, 0, 4, NodeType.Untangled),
                    new(4, 4, 2, NodeType.Tangled),
                    new(2, 2, 0, NodeType.Untangled),
                    new(4, 4, 0, NodeType.Untangled),
                    new(4, 4, 1, NodeType.Untangled),
                    new(6, 6, 2, NodeType.Untangled), 

                    new(1, 1, 0, NodeType.Untangled),
                    new(1, 0, 1, NodeType.Untangled),
                    new(1, 0, 2, NodeType.Untangled),
                    new(1, 2, 2, NodeType.Untangled),
                    new(1, 4, 0, NodeType.Untangled),
                    new(1, 4, 0, NodeType.Untangled),
                    new(1, 4, 3, NodeType.Tangled), 
                    new(1, 7, 1, NodeType.Untangled),

                    new(2, 0, 0, NodeType.Point),
                    new(2, 0, 1, NodeType.Untangled),
                    new(2, 0, 2, NodeType.Untangled),
                    new(2, 0, 3, NodeType.Untangled),
                    new(2, 2, 0, NodeType.Untangled),
                    new(2, 4, 4, NodeType.Tangled),
                    new(2, 4, 1, NodeType.Untangled),
                    new(2, 6, 2, NodeType.Untangled), 
                    new(2, 7, 2, NodeType.Point), 

                    new(3, 0, 2, NodeType.Untangled),
                    new(3, 0, 1, NodeType.Tangled),
                    new(3, 0, 3, NodeType.Untangled),
                    new(3, 0, 4, NodeType.Untangled),
                    new(3, 4, 1, NodeType.Untangled),
                    new(3, 4, 2, NodeType.Untangled),
                    new(3, 6, 0, NodeType.Untangled),
                    new(3, 7, 0, NodeType.Untangled),


                    //4-7
                    new(4, 0, 2, NodeType.Untangled),
                    new(4, 0, 3, NodeType.Untangled),
                    new(4, 0, 4, NodeType.Untangled),
                    new(4, 4, 2, NodeType.Tangled),
                    new(4, 2, 0, NodeType.Untangled),
                    new(4, 4, 0, NodeType.Untangled),
                    new(4, 4, 1, NodeType.Untangled),
                    new(4, 6, 2, NodeType.Untangled),

                    new(5, 0, 0, NodeType.Untangled),
                    new(5, 0, 1, NodeType.Untangled),
                    new(5, 0, 2, NodeType.Untangled),
                    new(5, 2, 2, NodeType.Untangled),
                    new(5, 4, 0, NodeType.Untangled),
                    new(5, 4, 0, NodeType.Untangled),
                    new(5, 4, 3, NodeType.Tangled),
                    new(5, 7, 1, NodeType.Untangled),

                    new(6, 0, 0, NodeType.Point),
                    new(6, 0, 1, NodeType.Untangled),
                    new(6, 0, 2, NodeType.Untangled),
                    new(6, 0, 3, NodeType.Untangled),
                    new(6, 2, 0, NodeType.Untangled),
                    new(6, 4, 4, NodeType.Tangled),
                    new(6, 4, 1, NodeType.Untangled),
                    new(6, 6, 2, NodeType.Untangled),
                    new(6, 7, 2, NodeType.Point),

                    new(7, 0, 2, NodeType.Untangled),
                    new(7, 0, 1, NodeType.Tangled),
                    new(7, 0, 3, NodeType.Untangled),
                    new(7, 0, 4, NodeType.Untangled),
                    new(7, 5, 1, NodeType.Untangled),
                    new(7, 5, 2, NodeType.Untangled),
                    new(7, 6, 0, NodeType.Untangled),
                    new(7, 7, 0, NodeType.Point),

                    //8-11
                    new(8, 0, 0, NodeType.Untangled),
                    new(8, 0, 2, NodeType.Untangled),
                    new(8, 0, 3, NodeType.Untangled),
                    new(8, 0, 4, NodeType.Untangled),
                    new(8, 2, 4, NodeType.Untangled),
                    new(8, 3, 0, NodeType.Untangled),
                    new(8, 3, 4, NodeType.Untangled),
                    new(8, 5, 0, NodeType.Untangled),
                    new(8, 5, 1, NodeType.Untangled),
                    new(8, 6, 1, NodeType.Untangled),
                    new(8, 7, 0, NodeType.Untangled),
                    new(8, 7, 1, NodeType.Untangled),

                    new(9, 0, 0, NodeType.Untangled),
                    new(9, 0, 1, NodeType.Untangled),
                    new(9, 0, 2, NodeType.Untangled),
                    new(9, 1, 0, NodeType.Untangled),
                    new(9, 2, 2, NodeType.Untangled),
                    new(9, 3, 0, NodeType.Untangled),
                    new(9, 4, 2, NodeType.Untangled),
                    new(9, 5, 0, NodeType.Untangled),
                    new(9, 5, 3, NodeType.Untangled),
                    new(9, 6, 0, NodeType.Untangled),

                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 2, NodeType.Untangled),
                    new(10, 0, 3, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 2, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 3, NodeType.Untangled),

                    new(11, 0, 2, NodeType.Untangled),
                    new(11, 0, 3, NodeType.Untangled),
                    new(11, 0, 4, NodeType.Untangled),
                    new(11, 1, 0, NodeType.Untangled),
                    new(11, 2, 4, NodeType.Untangled),
                    new(11, 3, 4, NodeType.Untangled),
                    new(11, 4, 0, NodeType.Untangled),
                    new(11, 4, 4, NodeType.Untangled),
                    new(11, 5, 0, NodeType.Untangled),
                    new(11, 6, 0, NodeType.Untangled),
                    new(11, 6, 1, NodeType.Untangled),
                    new(11, 6, 2, NodeType.Untangled),
                    new(11, 6, 3, NodeType.Untangled),

                    //12-15
                    new(8, 0, 0, NodeType.Untangled),
                    new(8, 0, 2, NodeType.Untangled),
                    new(8, 0, 3, NodeType.Untangled),
                    new(8, 0, 4, NodeType.Untangled),
                    new(8, 2, 4, NodeType.Untangled),
                    new(8, 3, 0, NodeType.Untangled),
                    new(8, 3, 4, NodeType.Untangled),
                    new(8, 5, 0, NodeType.Untangled),
                    new(8, 5, 1, NodeType.Untangled),
                    new(8, 6, 1, NodeType.Untangled),
                    new(8, 7, 0, NodeType.Untangled),
                    new(8, 7, 1, NodeType.Untangled),

                    new(9, 0, 0, NodeType.Untangled),
                    new(9, 0, 1, NodeType.Untangled),
                    new(9, 0, 2, NodeType.Untangled),
                    new(9, 1, 0, NodeType.Untangled),
                    new(9, 2, 2, NodeType.Untangled),
                    new(9, 3, 0, NodeType.Untangled),
                    new(9, 4, 2, NodeType.Untangled),
                    new(9, 5, 0, NodeType.Untangled),
                    new(9, 5, 3, NodeType.Untangled),
                    new(9, 6, 0, NodeType.Untangled),

                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 2, NodeType.Untangled),
                    new(10, 0, 3, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 2, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 0, NodeType.Untangled),
                    new(10, 0, 3, NodeType.Untangled),

                    new(11, 0, 2, NodeType.Untangled),
                    new(11, 0, 3, NodeType.Untangled),
                    new(11, 0, 4, NodeType.Untangled),
                    new(11, 1, 0, NodeType.Untangled),
                    new(11, 2, 4, NodeType.Untangled),
                    new(11, 3, 4, NodeType.Untangled),
                    new(11, 4, 0, NodeType.Untangled),
                    new(11, 4, 4, NodeType.Untangled),
                    new(11, 5, 0, NodeType.Untangled),
                    new(11, 6, 0, NodeType.Untangled),
                    new(11, 6, 1, NodeType.Untangled),
                    new(11, 6, 2, NodeType.Untangled),
                    new(11, 6, 3, NodeType.Untangled),

                }
            };

            return GraphBuilder.BuildGraph(structure);
        }
    }
}

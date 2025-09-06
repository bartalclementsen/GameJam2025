using System.Collections.Generic;
using Core.Loggers;
using UnityEngine;


namespace Jákup_Viljam.Models
{
    public class MusicNode
    {
        public int Bar;
        public int Beat;
        public int Line;
        public NodeType Type;
        public LineType LineType;
        public List<MusicNode> NextNodes = new();

        private readonly Core.Loggers.ILogger _logger;

        public MusicNode(int bar, int beat, int line, NodeType type, LineType lineType)
        {
            _logger = Game.Container.Resolve<ILoggerFactory>().Create(this);
            Bar = bar;
            Beat = beat;
            Line = line;
            Type = type;
            LineType = lineType;
        }

        public void PrintInfo()
        {
            string nexts = NextNodes.Count == 0 ? "None" :
                string.Join(", ", NextNodes.ConvertAll(n => $"({n.Bar},{n.Beat},{n.Line})"));
            _logger.Log($"Node: Bar {Bar}, Beat {Beat}, Line {Line}, Type {Type} => Next: {nexts}");
        }
    }
}

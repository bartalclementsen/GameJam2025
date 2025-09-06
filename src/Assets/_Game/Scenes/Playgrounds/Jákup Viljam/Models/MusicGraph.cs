using System.Collections.Generic;
using System.Text;
using Core.Loggers;
using UnityEngine;


namespace Jákup_Viljam.Models
{

    public class MusicGraph
    {
        private readonly Dictionary<(int, int, int), MusicNode> _nodes = new();
        private readonly int _totalBars;
        private readonly int _beatsPerBar;
        private readonly int _lines;
        private readonly Core.Loggers.ILogger _logger;

        public MusicGraph(List<MusicNode> inputNodes, int totalBars, int beatsPerBar, int lines, bool connectAdjacentLines = true)
        {
            _logger = Game.Container.Resolve<ILoggerFactory>().Create(this);

            _totalBars = totalBars;
            _beatsPerBar = beatsPerBar;
            _lines = lines;

            InitializeGraph(inputNodes, totalBars, beatsPerBar, lines, connectAdjacentLines);
        }

        public MusicNode GetNode(int bar, int beat, int line) =>
            _nodes.TryGetValue((bar, beat, line), out MusicNode node) ? node : null;

        public IEnumerable<MusicNode> AllNodes => _nodes.Values;

        public void PrintStaff()
        {
            StringBuilder[] rows = new StringBuilder[_lines];
            for (int l = 0; l < _lines; l++)
            {
                rows[l] = new StringBuilder();
            }

            for (int bar = 0; bar < _totalBars; bar++)
            {
                for (int beat = 0; beat < _beatsPerBar; beat++)
                {
                    for (int line = 0; line < _lines; line++)
                    {
                        MusicNode node = GetNode(bar, beat, line);
                        char sym = NodeSymbol(node?.Type ?? NodeType.Nothing);
                        rows[line].Append(' ').Append(sym).Append(' ');
                    }
                }

                if (bar < _totalBars - 1)
                {
                    for (int line = 0; line < _lines; line++)
                    {
                        rows[line].Append('|');
                    }
                }
            }

            // print top to bottom
            for (int line = _lines - 1; line >= 0; line--)
            {
                _logger.Log(rows[line].ToString());
            }
        }

        private char NodeSymbol(NodeType type) => type switch
        {
            NodeType.Untangled => '@',
            NodeType.Point => '#',
            NodeType.Powerup => '$',
            NodeType.Tangled => 'X',
            _ => '.'
        };

        private void InitializeGraph(List<MusicNode> inputNodes, int totalBars, int beatsPerBar, int lines, bool connectAdjacentLines)
        {
            foreach (MusicNode node in inputNodes)
            {
                _nodes[(node.Bar, node.Beat, node.Line)] = node;
            }

            foreach (MusicNode node in _nodes.Values)
            {
                int nextBeat = node.Beat + 1;
                int nextBar = node.Bar;

                if (nextBeat >= beatsPerBar)
                {
                    nextBeat = 0;
                    nextBar = node.Bar + 1;
                }

                if (nextBar >= totalBars)
                {
                    continue;
                }

                int minLine = connectAdjacentLines ? Mathf.Max(0, node.Line - 1) : node.Line;
                int maxLine = connectAdjacentLines ? Mathf.Min(lines - 1, node.Line + 1) : node.Line;

                for (int l = minLine; l <= maxLine; l++)
                {
                    if (_nodes.TryGetValue((nextBar, nextBeat, l), out MusicNode target))
                    {
                        node.NextNodes.Add(target);
                    }
                }
            }
        }

    }
}

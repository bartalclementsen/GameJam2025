using System;
using System.Collections.Generic;


namespace Jákup_Viljam.Models
{
    public class GraphBuilder
    {
        private static readonly System.Random _rng = new();

        public static MusicGraph BuildGraph(GraphStructure structure)
        {
            List<MusicNode> nodes = new();

            // Create full 0-based grid
            for (int bar = 0; bar < structure.Bars; bar++)
            {
                for (int beat = 0; beat < structure.BeatsPerBar; beat++)
                {
                    for (int line = 0; line < structure.Lines; line++)
                    {
                        nodes.Add(new MusicNode(bar, beat, line, NodeType.Nothing));
                    }
                }
            }

            // Apply any special nodes
            foreach (MusicNode n in structure.SpecialNodes)
            {
                MusicNode node = nodes.Find(x => x.Bar == n.Bar && x.Beat == n.Beat && x.Line == n.Line);
                if (node != null)
                {
                    node.Type = n.Type;
                    if (node.Type == NodeType.Tangled)
                    {
                        Array values = Enum.GetValues(typeof(LineType));
                        node.LineType = (LineType)values.GetValue(_rng.Next(values.Length))!;
                    }
                }
            }

            return new MusicGraph(structure, nodes, structure.Bars, structure.BeatsPerBar, structure.Lines);
        }
    }
}

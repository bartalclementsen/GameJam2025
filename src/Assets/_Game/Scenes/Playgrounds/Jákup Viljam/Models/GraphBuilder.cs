using System;
using System.Collections.Generic;
using Unity.VisualScripting;


namespace Jákup_Viljam.Models
{
    public class GraphBuilder
    {
        private static readonly Random _rng = new();

        public static MusicGraph BuildGraph(GraphStructure structure)
        {
            // Generate all nodes
            List<MusicNode> nodes = GenerateMusicNodes(structure);

            // Apply any special nodes
            OverwriteSpecialNodes(structure, nodes);

            return new MusicGraph(structure, nodes, structure.Bars, structure.BeatsPerBar, structure.Lines);
        }

        public static MusicGraph BuildDynamicStructure(GraphStructure structure)
        {
            // Generate all nodes
            List<MusicNode> nodes = GenerateMusicNodes(structure);

            // Generate a random path
            List<MusicNode> path = GeneratePath(new MusicGraph(structure, nodes, structure.Bars, structure.BeatsPerBar, structure.Lines));
            AssignPathTypes(path);

            // Apply any special nodes
            OverwriteSpecialNodes(structure, nodes);

            // Initialize graph
            MusicGraph graph = new(structure, nodes, structure.Bars, structure.BeatsPerBar, structure.Lines);

            // Scatter untangled nodes
            ScatterUntangled(graph, new HashSet<MusicNode>(path));

            return graph;
        }

        public static MusicGraph BuildStaticStructure(GraphStructure structure)
        {
            // Generate all nodes
            structure.SpecialNodes = GenerateStaticNotes();
            return BuildGraph(structure);
        }

        private static List<MusicNode> GenerateStaticNotes()
        {
            return new List<MusicNode>
                {
                    //0-3 --------------------------------------------- bar / beat / line
                    new(0, 0, 2, NodeType.Untangled),
                    new(0, 0, 3, NodeType.Untangled),
                    new(0, 0, 4, NodeType.Untangled),
                    new(0, 6, 1, NodeType.Tangled),
                    //new(0, 2, 0, NodeType.Untangled),
                    new(0, 4, 0, NodeType.Untangled),
                    new(0, 4, 1, NodeType.Untangled),
                    new(0, 6, 2, NodeType.Untangled),

                    new(1, 1, 0, NodeType.Untangled),
                    new(1, 0, 1, NodeType.Untangled),
                    //new(1, 0, 2, NodeType.Untangled),
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
                    new(4, 6, 1, NodeType.Tangled),
                    new(4, 4, 0, NodeType.Untangled),
                    new(4, 4, 1, NodeType.Untangled),
                    new(4, 6, 2, NodeType.Untangled),

                    new(5, 1, 0, NodeType.Untangled),
                    new(5, 0, 1, NodeType.Untangled),
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


                    //8-11 --------------------------------------------- bar / beat / line
                    new(8, 0, 0, NodeType.Untangled),
                    new(8, 2, 0, NodeType.Untangled),
                    new(8, 0, 2, NodeType.Tangled),
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
                    new(9, 2, 1, NodeType.Tangled),
                    new(9, 2, 2, NodeType.Untangled),
                    new(9, 3, 0, NodeType.Untangled),
                    new(9, 4, 2, NodeType.Untangled),
                    new(9, 5, 0, NodeType.Untangled),
                    new(9, 5, 3, NodeType.Untangled),
                    new(9, 6, 0, NodeType.Untangled),

                    new(10, 0, 1, NodeType.Untangled),
                    new(10, 0, 2, NodeType.Untangled),
                    new(10, 0, 3, NodeType.Untangled),
                    new(10, 1, 0, NodeType.Untangled),
                    new(10, 1, 0, NodeType.Tangled),
                    new(10, 2, 1, NodeType.Untangled),
                    new(10, 3, 0, NodeType.Untangled),
                    new(10, 4, 1, NodeType.Untangled),
                    new(10, 5, 2, NodeType.Untangled),
                    new(10, 6, 0, NodeType.Untangled),
                    new(10, 7, 0, NodeType.Untangled),
                    new(10, 7, 3, NodeType.Untangled),

                    new(11, 0, 2, NodeType.Untangled),
                    new(11, 0, 3, NodeType.Untangled),
                    new(11, 0, 4, NodeType.Untangled),
                    new(11, 1, 0, NodeType.Untangled),
                    new(11, 2, 4, NodeType.Untangled),
                    new(11, 3, 4, NodeType.Untangled),
                    new(11, 4, 0, NodeType.Untangled),
                    new(11, 4, 4, NodeType.Untangled),
                    new(11, 5, 0, NodeType.Untangled),
                    new(11, 6, 0, NodeType.Tangled),
                    new(11, 6, 1, NodeType.Untangled),
                    new(11, 6, 2, NodeType.Untangled),
                    new(11, 6, 3, NodeType.Untangled),

                    //12-15
                    new(12, 0, 0, NodeType.Untangled),
                    new(12, 0, 2, NodeType.Untangled),
                    new(12, 3, 2, NodeType.Tangled),
                    new(12, 0, 3, NodeType.Untangled),
                    new(12, 0, 4, NodeType.Untangled),
                    new(12, 2, 4, NodeType.Untangled),
                    new(12, 3, 0, NodeType.Untangled),
                    new(12, 3, 4, NodeType.Untangled),
                    new(12, 5, 0, NodeType.Untangled),
                    new(12, 5, 1, NodeType.Untangled),
                    new(12, 6, 1, NodeType.Untangled),
                    new(12, 7, 0, NodeType.Untangled),
                    new(12, 7, 1, NodeType.Untangled),

                    new(13, 0, 0, NodeType.Untangled),
                    new(13, 0, 1, NodeType.Untangled),
                    new(13, 0, 2, NodeType.Untangled),
                    new(13, 1, 0, NodeType.Untangled),
                    new(13, 2, 1, NodeType.Tangled),
                    new(13, 2, 2, NodeType.Untangled),
                    new(13, 3, 0, NodeType.Untangled),
                    new(13, 4, 2, NodeType.Untangled),
                    new(13, 5, 0, NodeType.Untangled),
                    new(13, 5, 3, NodeType.Untangled),
                    new(13, 6, 0, NodeType.Untangled),

                    new(14, 0, 1, NodeType.Untangled),
                    new(14, 0, 2, NodeType.Untangled),
                    new(14, 0, 3, NodeType.Untangled),
                    new(14, 1, 0, NodeType.Untangled),
                    new(14, 1, 0, NodeType.Tangled),
                    new(14, 2, 1, NodeType.Untangled),
                    new(14, 3, 0, NodeType.Untangled),
                    new(14, 4, 1, NodeType.Untangled),
                    new(14, 5, 2, NodeType.Untangled),
                    new(14, 6, 0, NodeType.Untangled),
                    new(14, 7, 0, NodeType.Untangled),
                    new(14, 7, 3, NodeType.Untangled),

                    new(15, 0, 2, NodeType.Untangled),
                    new(15, 0, 3, NodeType.Untangled),
                    new(15, 0, 4, NodeType.Untangled),
                    new(15, 1, 0, NodeType.Untangled),
                    new(15, 2, 4, NodeType.Untangled),
                    new(15, 3, 4, NodeType.Untangled),
                    new(15, 4, 0, NodeType.Untangled),
                    new(15, 4, 4, NodeType.Untangled),
                    new(15, 5, 0, NodeType.Untangled),
                    new(15, 6, 0, NodeType.Tangled),
                    new(15, 6, 1, NodeType.Untangled),
                    new(15, 6, 2, NodeType.Untangled),
                    new(15, 6, 3, NodeType.Untangled),

                };
        }

        private static List<MusicNode> GenerateMusicNodes(GraphStructure structure)
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

            return nodes;
        }

        private static void OverwriteSpecialNodes(GraphStructure structure, List<MusicNode> nodes)
        {
            foreach (MusicNode n in structure.SpecialNodes)
            {
                MusicNode node = nodes.Find(x => x.Bar == n.Bar && x.Beat == n.Beat && x.Line == n.Line);
                if (node != null)
                {
                    node.Type = n.Type;
                    node.LineType = LineType.OnLine;
                    if (node.Type == NodeType.Tangled)
                    {
                        RandomizeUpDownTangle(node);
                    }
                }
            }
        }

        private static void RandomizeUpDownTangle(MusicNode node)
        {
            Array values = Enum.GetValues(typeof(LineType));
            int index = _rng.Next(1, values.Length);
            node.LineType = (LineType)values.GetValue(index)!;
        }

        private static List<MusicNode> GeneratePath(MusicGraph graph, int startLine = -1)
        {
            Random rng = new();
            List<MusicNode> path = new();

            int line = startLine >= 0 ? startLine : rng.Next(graph.GraphStructure.Lines);

            for (int bar = 0; bar < graph.GraphStructure.Bars; bar++)
            {
                for (int beat = 0; beat < graph.GraphStructure.BeatsPerBar; beat++)
                {
                    MusicNode node = graph.GetNode(bar, beat, line);
                    if (node != null)
                    {
                        path.Add(node);
                    }

                    // Decide next line (stay, up, down)
                    int shift = rng.Next(-1, 2); // -1, 0, +1
                    line = Math.Clamp(line + shift, 0, graph.GraphStructure.Lines - 1);
                }
            }

            return path;
        }

        private static void AssignPathTypes(List<MusicNode> path, double tangledChance = 0.24, double pointChance = 0.1)
        {
            Random rng = new();

            foreach (MusicNode node in path)
            {
                double roll = rng.NextDouble();
                if (roll < tangledChance)
                {
                    node.Type = NodeType.Tangled;
                    RandomizeUpDownTangle(node);
                }
                else if (roll < tangledChance + pointChance )
                {
                    //TODO
                }
            }
        }

        private static void ScatterUntangled(MusicGraph graph, HashSet<MusicNode> pathNodes, double scatterChance = 0.15)
        {
            Random rng = new();

            foreach (MusicNode node in graph.AllNodes)
            {
                if (pathNodes.Contains(node)) continue; // skip path nodes

                if (rng.NextDouble() < scatterChance)
                {
                    node.Type = NodeType.Untangled;
                }
            }
        }
    }
}

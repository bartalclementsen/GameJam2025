using System.Collections.Generic;
using Jákup_Viljam.Models;
using UnityEngine;

public class GraphPrinter : MonoBehaviour
{
    void Start()
    {
        GraphStructure structure = new GraphStructure
        {
            Lines = 5,
            BeatsPerBar = 8,
            Bars = 16,
            SpecialNodes = new List<MusicNode>
            {
                new(1, 8, 2, NodeType.Tangled, LineType.OnLine),
                new(2, 4, 1, NodeType.Point, LineType.OnLine),
                new(5, 5, 3, NodeType.Untangled, LineType.AboveLine),
                new(8, 8, 0, NodeType.Tangled, LineType.BelowLine),
                new(2, 4, 5, NodeType.Untangled, LineType.OnLine),
                new(0, 5, 1, NodeType.Point, LineType.OnLine),
                new(8, 8, 5, NodeType.Powerup, LineType.OnLine),
            }
        };

        // Build graph from structure
        MusicGraph graph = GraphBuilder.BuildGraph(structure);

        // Print it
        graph.PrintStaff();

        PrintSpecificNodes(graph);
    }

    void Update()
    {

    }

    void PrintSpecificNodes(MusicGraph graph)
    {
        MusicNode node1 = graph.GetNode(bar: 0, beat: 0, line: 0);
        node1?.PrintInfo();

        MusicNode node2 = graph.GetNode(bar: 0, beat: 0, line: 2);
        node2?.PrintInfo();

        MusicNode node3 = graph.GetNode(bar: 0, beat: 0, line: 4);
        node3?.PrintInfo();

        MusicNode node4 = graph.GetNode(bar: 0, beat: 0, line: 2);
        node4?.PrintInfo();
    }
}

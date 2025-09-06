using System.Collections.Generic;


namespace Jákup_Viljam.Models
{
    [System.Serializable]
    public class GraphStructure
    {
        public int Lines = 5;           // number of staff lines
        public int BeatsPerBar = 8;     // nodes per bar
        public int Bars = 16;           // total bars

        // Optional: specify special nodes (which are the NodeTypes)
        public List<MusicNode> SpecialNodes = new();
    }
}

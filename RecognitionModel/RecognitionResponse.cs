using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class RecognitionResponse
    {
        public Dictionary<string, int> FoundObjects { get; set; }
        public int Percent { get; set; }
        public List<List<float>> Corners { get; set; }
        public RecognitionResponse()
        {
            FoundObjects = new Dictionary<string, int>();
            Corners = new List<List<float>>();
        }
    }
}

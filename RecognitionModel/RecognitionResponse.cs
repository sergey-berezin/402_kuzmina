using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Models
{
    public class RecognitionResponse
    {
        public Dictionary<string, int> FoundObjects { get; set; }
        public int Percent { get; set; }
        public List<RecognitionObject> Objects { get; set; }
        public byte[] Image { get; set; }
        public RecognitionResponse()
        {
            FoundObjects = new Dictionary<string, int>();
            Objects = new List<RecognitionObject>();
        }
    }
}

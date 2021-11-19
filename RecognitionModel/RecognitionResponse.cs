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
        public Dictionary<List<float>, string> Corners { get; set; }
        public Bitmap Image { get; set; }
        public RecognitionResponse()
        {
            FoundObjects = new Dictionary<string, int>();
            Corners = new Dictionary<List<float>, string>();
        }
    }
}

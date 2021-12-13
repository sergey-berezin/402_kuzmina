using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class RecognitionObject
    {
        public string Label { get; set; }
        public float[] Corners { get; set; }
        public RecognitionObject(string label, float[] corners)
        {
            Label = label;
            Corners = corners;
        }
    }
}

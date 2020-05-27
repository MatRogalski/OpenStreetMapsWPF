using System;
using System.Collections.Generic;
using System.Text;

namespace Router.Model
{
    public class TravelTimesMatrixModel
    {
        public float[][] durations { get; set; }
        public float[][] distances { get; set; }
        public Waypoint[] destinations { get; set; }
        public Waypoint[] sources { get; set; }
        public string code { get; set; }
    }
    
}

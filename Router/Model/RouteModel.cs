using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Router.Model
{
    public class RouteModel
    {
        public MultiPoint MultiPoint { get; set; }
        public double Distance { get; set; }
        public double Time { get; set; }
    }
}

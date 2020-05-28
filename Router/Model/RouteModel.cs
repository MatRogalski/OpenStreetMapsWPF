using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Router.Model
{
    public class RouteModel
    {
        public MultiPoint MultiPoint { get; set; }
        public GeoJSON.Net.Geometry.MultiPoint MultiPointGeoJsonNet { get; set; }
        public GeoJSON.Net.Geometry.LineString LineString { get; set; }
        public GeoJSON.Net.Geometry.MultiLineString MultiLineString { get; set; }
        public double Distance { get; set; }
        public double Time { get; set; }
        public Waypoint[] Waypoints { get; set; }
    }
}

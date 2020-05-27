using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Router.Model
{
    public class OsrmJsonRouteModel
    {
        public Route[] routes { get; set; }
        public Waypoint[] waypoints { get; set; }

        [JsonProperty("trips")]
        private Route[] trips { set { routes = value; } }

        public RouteModel ToRouteModel()
        {
            var multipoint = GetNetTopologySuiteMultiPoint();
            if(multipoint == null)
            {
                throw new ArgumentNullException("multipoint");
            }
            var multipointGeoJsonNet = GetGeoJsonNetMultiPoint();
            if (multipointGeoJsonNet == null)
            {
                throw new ArgumentNullException("multipointGeoJsonNet");
            }

            double distance, time;
            if(routes != null && routes.Length > 0)
            {
                distance = routes[0].distance;
                time = routes[0].duration;
            }
            else
            {
                throw new ArgumentException();
            }

            return new RouteModel()
            {
                MultiPoint = multipoint,
                MultiPointGeoJsonNet = multipointGeoJsonNet,
                Distance = distance,
                Time = time
            };
        }

        private MultiPoint GetNetTopologySuiteMultiPoint()
        {
            if(routes != null && routes.Length > 0)
            {
                var route = routes[0];
                if(route.geometry != null)
                {
                    if(route.geometry.coordinates != null && route.geometry.coordinates.Length > 0)
                    {
                        List<Point> points = new List<Point>();
                        foreach(float[] coordinate in route.geometry.coordinates)
                        {
                            float longitude = coordinate[0];
                            float latitude = coordinate[1];
                            Point point = new Point(longitude, latitude);
                            points.Add(point);
                        }
                        return new MultiPoint(points.ToArray());
                    }
                }
            }

            throw new ArgumentException();
        }

        private GeoJSON.Net.Geometry.MultiPoint GetGeoJsonNetMultiPoint()
        {
            if (routes != null && routes.Length > 0)
            {
                var route = routes[0];
                if (route.geometry != null)
                {
                    if (route.geometry.coordinates != null && route.geometry.coordinates.Length > 0)
                    {
                        List<GeoJSON.Net.Geometry.Point> points = new List<GeoJSON.Net.Geometry.Point>();
                        foreach (float[] coordinate in route.geometry.coordinates)
                        {
                            float longitude = coordinate[0];
                            float latitude = coordinate[1];
                            GeoJSON.Net.Geometry.Position position = new GeoJSON.Net.Geometry.Position(latitude, longitude);
                            GeoJSON.Net.Geometry.Point point = new GeoJSON.Net.Geometry.Point(position);
                            points.Add(point);
                        }
                        return new GeoJSON.Net.Geometry.MultiPoint(points);
                    }
                }
            }

            throw new ArgumentException();
        }



    }


    public class Waypoint
    {
        public string hint { get; set; }
        public float distance { get; set; }
        public string name { get; set; }
        public float[] location { get; set; }
    }

    public class Route
    {
        public GeometryOsrm geometry { get; set; }
        public Legs[] legs { get; set; }
        public string weightName { get; set; }
        public double weight { get; set; }
        public double duration { get; set; }
        public double distance { get; set; }
    }

    public class GeometryOsrm
    {
        public string type { get; set; }
        // format: [longitude, latitude]
        public float[][] coordinates { get; set; }
    }
    
    public class Legs
    {
    }


}

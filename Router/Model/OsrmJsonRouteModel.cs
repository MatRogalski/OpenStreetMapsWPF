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

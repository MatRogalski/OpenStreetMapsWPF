using MapDisplayApp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using MapDisplayApp.Proxies;

namespace MapDisplayApp.APIHelpers
{
    public static class OsrmAPIHelper
    {
        public static OsrmJsonRouteModel GetRouteBetweenPoints(params Coordinate[] coordinates)
        {
            string coordinatesString = GetStringFromCoordinates(coordinates);

            string uri = $"http://router.project-osrm.org/route/v1/driving/{coordinatesString}?geometries=geojson&overview=full";         
            string html = HttpProxy.DownloadResource(uri);
            OsrmJsonRouteModel parsed = JsonConvert.DeserializeObject<OsrmJsonRouteModel>(html);
            return parsed;
        }

        public static OsrmJsonRouteModel GetOptimalRoute(Coordinate first, Coordinate last, params Coordinate[] intermediates)
        {
            Coordinate[] coordinates = new Coordinate[intermediates.Length + 2];
            coordinates[0] = first;
            Array.Copy(intermediates, 0, coordinates, 1, intermediates.Length);
            coordinates[coordinates.Length - 1] = last;
            string coordinateString = GetStringFromCoordinates(coordinates);

            string uri = $"http://router.project-osrm.org/trip/v1/driving/{coordinateString}?roundtrip=false&source=first&destination=last&geometries=geojson&overview=full";
            string html = HttpProxy.DownloadResource(uri);
            OsrmJsonRouteModel parsed = JsonConvert.DeserializeObject<OsrmJsonRouteModel>(html);
            return parsed;
        }

        private static string GetStringFromCoordinates(Coordinate[] coordinates)
        {
            string coordinatesString = "";
            foreach (var coordinate in coordinates)
            {
                if (coordinate != null)
                    coordinatesString += $"{coordinate.longitude.ToString(CultureInfo.InvariantCulture)},{coordinate.latitude.ToString(CultureInfo.InvariantCulture)};";
            }
            coordinatesString = coordinatesString.Substring(0, coordinatesString.Length - 1);
            return coordinatesString;
        }

    }

    public class Coordinate
    {
        public float latitude { get; set; }
        public float longitude { get; set; }        

        public Coordinate(float latitude, float longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;            
        }
    }
}

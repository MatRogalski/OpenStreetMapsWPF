using GeoJSON.Net.Contrib.MsSqlSpatial;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using MapDisplayApp.Proxies;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MapDisplayApp.APIHelpers
{
    public static class MapboxAPIHelper
    {
        private const string API_KEY = "pk.eyJ1IjoibWlsdGVuNiIsImEiOiJja2FsOW53YmIwOXRlMnhteWNyczdyMzF0In0.8w1TkiipRjW35bNlfH7kYQ";

        public static Polygon GetIsochroneAsPolygon(Position position, int contourMinutes)
        {
            FeatureCollection isochronesFeatureCollection = GetIsochronesFeatureCollection(position, contourMinutes);
            var polygon = isochronesFeatureCollection.Features[0].Geometry;
            if (polygon.Type == GeoJSON.Net.GeoJSONObjectType.Polygon)
                return (Polygon)polygon;
            else
                throw new Exception();
        }

        public static FeatureCollection GetIsochronesFeatureCollection(Position position, params int[] contoursMinutes)
        {
            if (contoursMinutes.Length > 4 || contoursMinutes.Length < 1)
                throw new ArgumentOutOfRangeException();

            string positionString = GetStringFromPosition(position);
            string minutesString = GetStringFromMinutes(contoursMinutes);

            string uri = $"https://api.mapbox.com/isochrone/v1/mapbox/driving/{positionString}?contours_minutes={minutesString}&polygons=true&access_token={API_KEY}";
            string json = HttpProxy.DownloadResource(uri);
            FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json);
            return featureCollection;
        }

        public static bool CheckIfPointIsInsidePolygon(Polygon polygon, Point point)
        {
            SqlGeometry polygonGeometry = polygon.ToSqlGeometry();
            polygonGeometry = polygonGeometry.MakeValid();
            SqlGeometry pointGeometry = point.ToSqlGeometry();
            pointGeometry = pointGeometry.MakeValid();
            return pointGeometry.STIntersects(polygonGeometry).Value;
        }

        private static string GetStringFromMinutes(params int[] minutes)
        {
            string minutesString = "";
            foreach (var minute in minutes)
            {
                minutesString += $"{minute.ToString()},";
            }
            minutesString = minutesString.Substring(0, minutesString.Length - 1);
            return minutesString;
        }

        private static string GetStringFromPosition(Position position)
        {
            return $"{position.Longitude.ToString(CultureInfo.InvariantCulture)},{position.Latitude.ToString(CultureInfo.InvariantCulture)}";
        }

    }
}

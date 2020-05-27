using GeoJSON.Net.Contrib.MsSqlSpatial;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Router.Proxies;
using Microsoft.SqlServer.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Router.Utils;
using Router.Model;

namespace Router.APIHelpers
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

            string positionString = StringUtils.GetStringFromPositions(position);
            string minutesString = StringUtils.GetStringFromMinutes(contoursMinutes);

            string uri = $"https://api.mapbox.com/isochrone/v1/mapbox/driving/{positionString}?contours_minutes={minutesString}&polygons=true&access_token={API_KEY}";
            string json = HttpProxy.DownloadResource(uri);
            FeatureCollection featureCollection = JsonConvert.DeserializeObject<FeatureCollection>(json);
            return featureCollection;
        }

        public static TravelTimesMatrixModel GetTravelTimesMatrix(Position source, params Position[] desinations)
        {
            Position[] positions = new Position[desinations.Length + 1];
            positions[0] = source;
            Array.Copy(desinations, 0, positions, 1, desinations.Length);
            string positionString = StringUtils.GetStringFromPositions(positions);

            string uri = $"https://api.mapbox.com/directions-matrix/v1/mapbox/driving/{positionString}?sources=0&annotations=duration,distance&access_token={API_KEY}";
            string json = HttpProxy.DownloadResource(uri);
            TravelTimesMatrixModel parsed = JsonConvert.DeserializeObject<TravelTimesMatrixModel>(json);
            return parsed;
        }

        

        

    }
}

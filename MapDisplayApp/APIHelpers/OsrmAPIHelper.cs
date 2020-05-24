using MapDisplayApp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using MapDisplayApp.Proxies;
using Nominatim.API.Models;
using GeoJSON.Net.Geometry;
using MapDisplayApp.Utils;

namespace MapDisplayApp.APIHelpers
{
    public static class OsrmAPIHelper
    {
        public static OsrmJsonRouteModel GetSimpleRoute(params Coordinate[] coordinates)
        {
            string coordinatesString = StringUtils.GetStringFromCoordinates(coordinates);

            string uri = $"http://router.project-osrm.org/route/v1/driving/{coordinatesString}?geometries=geojson&overview=full";         
            string html = HttpProxy.DownloadResource(uri);
            OsrmJsonRouteModel parsed = JsonConvert.DeserializeObject<OsrmJsonRouteModel>(html);
            return parsed;
        }

        public static OsrmJsonRouteModel GetSimpleRoute(string sourceQuery, string destinationQuery, params Coordinate[] intermediates)
        {
            Coordinate sourceCoordinate = GetCoordinateForAdress(sourceQuery);
            Coordinate destinationCoordinate = GetCoordinateForAdress(destinationQuery);
            Coordinate[] coordinates = GetCoordinatesArray(sourceCoordinate, destinationCoordinate, intermediates);
            return GetSimpleRoute(coordinates);
        }

        public static OsrmJsonRouteModel GetOptimalRoute(Coordinate first, Coordinate last, params Coordinate[] intermediates)
        {
            Coordinate[] coordinates = GetCoordinatesArray(first, last, intermediates);
            string coordinateString = StringUtils.GetStringFromCoordinates(coordinates);
            string uri = $"http://router.project-osrm.org/trip/v1/driving/{coordinateString}?roundtrip=false&source=first&destination=last&geometries=geojson&overview=full";
            string html = HttpProxy.DownloadResource(uri);
            OsrmJsonRouteModel parsed = JsonConvert.DeserializeObject<OsrmJsonRouteModel>(html);
            return parsed;
        }

        public static OsrmJsonRouteModel GetOptimalRoute(string sourceQuery, string destinationQuery, params Coordinate[] intermediates)
        {
            Coordinate sourceCoordinate = GetCoordinateForAdress(sourceQuery);
            Coordinate destinationCoordinate = GetCoordinateForAdress(destinationQuery);
            return GetOptimalRoute(sourceCoordinate, destinationCoordinate, intermediates);
        }

        public static TravelTimesMatrixModel GetTravelTimesMatrix(Position source, params Position[] desinations)
        {
            Position[] positions = new Position[desinations.Length + 1];
            positions[0] = source;
            Array.Copy(desinations, 0, positions, 1, desinations.Length);
            string positionString = StringUtils.GetStringFromPositions(positions);

            string uri = $"http://router.project-osrm.org/table/v1/driving/{positionString}?sources=0";
            string json = HttpProxy.DownloadResource(uri);
            TravelTimesMatrixModel parsed = JsonConvert.DeserializeObject<TravelTimesMatrixModel>(json);
            return parsed;
        }

        private static Coordinate GetCoordinateForAdress(string addressquery)
        {
            var addressDetails = NominatimAPIHelper.GetAdressDetails(addressquery);
            return new Coordinate((float)addressDetails.Latitude, (float)addressDetails.Longitude);
        }

        

        private static Coordinate[] GetCoordinatesArray(Coordinate source, Coordinate destination, Coordinate[] intermediates)
        {
            Coordinate[] coordinates = new Coordinate[intermediates.Length + 2];
            coordinates[0] = source;
            Array.Copy(intermediates, 0, coordinates, 1, intermediates.Length);
            coordinates[coordinates.Length - 1] = destination;
            return coordinates;
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

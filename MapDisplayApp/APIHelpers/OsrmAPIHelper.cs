using MapDisplayApp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using MapDisplayApp.Proxies;
using Nominatim.API.Models;

namespace MapDisplayApp.APIHelpers
{
    public static class OsrmAPIHelper
    {
        public static OsrmJsonRouteModel GetSimpleRoute(params Coordinate[] coordinates)
        {
            string coordinatesString = GetStringFromCoordinates(coordinates);

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
            string coordinateString = GetStringFromCoordinates(coordinates);
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

        private static Coordinate GetCoordinateForAdress(string addressquery)
        {
            var addressDetails = GetAdressDetails(addressquery);
            return new Coordinate((float)addressDetails.Latitude, (float)addressDetails.Longitude);
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

        public static Coordinate[] GetCoordinatesArray(Coordinate source, Coordinate destination, Coordinate[] intermediates)
        {
            Coordinate[] coordinates = new Coordinate[intermediates.Length + 2];
            coordinates[0] = source;
            Array.Copy(intermediates, 0, coordinates, 1, intermediates.Length);
            coordinates[coordinates.Length - 1] = destination;
            return coordinates;
        }

        private static GeocodeResponse GetAdressDetails(string query)
        {
            Nominatim.API.Geocoders.ForwardGeocoder geocoder = new Nominatim.API.Geocoders.ForwardGeocoder();
            Nominatim.API.Models.ForwardGeocodeRequest request = new Nominatim.API.Models.ForwardGeocodeRequest()
            {
                queryString = query,
                LimitResults = 1,
                ShowGeoJSON = true
            };
            var geocodeResponses = geocoder.Geocode(request);
            geocodeResponses.Wait();
            return geocodeResponses.Result == null ? geocodeResponses.Result[0] : null;
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

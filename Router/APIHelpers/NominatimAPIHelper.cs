using GeoJSON.Net.Geometry;
using Nominatim.API.Geocoders;
using Nominatim.API.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Router.APIHelpers
{
    public static class NominatimAPIHelper
    {
        public static GeocodeResponse GetAdressDetails(string query)
        {
            ForwardGeocoder geocoder = new ForwardGeocoder();
            ForwardGeocodeRequest request = new ForwardGeocodeRequest()
            {
                queryString = query,
                LimitResults = 1,
                ShowGeoJSON = true
            };
            var geocodeResponses = geocoder.Geocode(request);
            geocodeResponses.Wait();
            return geocodeResponses.Result == null ? null : geocodeResponses.Result[0];
        }

        public static Position GetPositionForAddress(string addressquery)
        {
            var addressDetails = GetAdressDetails(addressquery);

            // API requires exactly 7 numbers after comma
            //double latitude = Math.Round(addressDetails.Latitude, 7);
            //double longitude = Math.Round(addressDetails.Longitude, 7);

            return new Position(addressDetails.Latitude, addressDetails.Longitude);
        }
    }
}

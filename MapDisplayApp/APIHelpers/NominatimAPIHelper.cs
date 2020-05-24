using Nominatim.API.Geocoders;
using Nominatim.API.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MapDisplayApp.APIHelpers
{
    public static class NominatimAPIHelper
    {
        public static GeocodeResponse GetAdressDetails(string query)
        {
            ForwardGeocoder geocoder = new Nominatim.API.Geocoders.ForwardGeocoder();
            ForwardGeocodeRequest request = new ForwardGeocodeRequest()
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
}

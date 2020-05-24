using GeoJSON.Net.Geometry;
using MapDisplayApp.APIHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MapDisplayApp.Utils
{
    public static class StringUtils
    {
        public static string GetStringFromMinutes(params int[] minutes)
        {
            string minutesString = "";
            foreach (var minute in minutes)
            {
                minutesString += $"{minute.ToString()},";
            }
            minutesString = minutesString.Substring(0, minutesString.Length - 1);
            return minutesString;
        }

        public static string GetStringFromPositions(params Position[] positions)
        {
            string positionsString = "";
            foreach (var position in positions)
            {
                if (position != null)
                    positionsString += $"{position.Longitude.ToString(CultureInfo.InvariantCulture)},{position.Latitude.ToString(CultureInfo.InvariantCulture)};";
            }
            positionsString = positionsString.Substring(0, positionsString.Length - 1);
            return positionsString;
        }


    }
}

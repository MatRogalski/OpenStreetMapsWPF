using GeoJSON.Net.Contrib.MsSqlSpatial;
using GeoJSON.Net.Geometry;
using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Router.Utils
{
    public static class GeometryUtils
    {
        public static bool CheckIfPointIsInsidePolygon(Polygon polygon, Point point)
        {
            SqlGeometry polygonGeometry = polygon.ToSqlGeometry();
            polygonGeometry = polygonGeometry.MakeValid();
            SqlGeometry pointGeometry = point.ToSqlGeometry();
            pointGeometry = pointGeometry.MakeValid();
            return pointGeometry.STIntersects(polygonGeometry).Value;
        }
    }
}

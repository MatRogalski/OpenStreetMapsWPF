using DbConnector.Repositories;
using DbModel;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Contrib.MsSqlSpatial;
using Microsoft.SqlServer.Types;
using Router.APIHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Router.Model;

namespace DatabaseInitialFeeder
{
	class Program
	{
		static void Main(string[] args)
		{

			//InsertDataFromFileToDb(@$"C:\OSM\mazowieckie_100.csv");
			InsertAggregatedPointsToDb();
		}

		private static void InsertDataFromFileToDb(string fileName)
		{
			string[] lines = File.ReadAllLines(fileName);
			var repo = new LocalizationPointRepository();

			foreach (string line in lines)
			{
				string[] splitted = line.Split(",", StringSplitOptions.None);

				if (splitted.Count() >= 9
					&& double.TryParse(splitted[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double longitude)
					&& double.TryParse(splitted[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double latitude))
				{


					var localizationPoint = new LocalizationPointDto()
					{
						Point = new Point(new Position(latitude,longitude)),
						Number = splitted[2],
						Street = splitted[3],
						City = splitted[5],
						District = splitted[6],
						Region = splitted[7],
						PostalCode = splitted[8],
						StaticScore = 0,
						InnerDistance = 0,
						InnerTime = 0
					};

					repo.Add(localizationPoint);
				}
			}
			repo.SaveChanges();
		}

		private static void InsertAggregatedPointsToDb()
		{
			var repo = new LocalizationPointRepository();
			var points = repo.GetWithoutAggregated();

			for (int i = 0; i < points.Count(); i++)
			{
				if (points.ElementAt(i).ParentPointId != null || points.ElementAt(i).StaticScore != 0)
					continue;

				var isochrone = MapboxAPIHelper.GetIsochroneAsPolygon((Position)points.ElementAt(i).Point.Coordinates, 15);
				SqlGeography isochroneSqlGeography = isochrone.ToSqlGeography().MakeValid();
				isochroneSqlGeography = GetCorrectlyOrientedGeography(isochroneSqlGeography);

				// take points that are not parent or child points in aggregation
				// TODO: how to get modified points after updating parentID field
				var pointsWithoutParentPoint = points.Where(i => i.ParentPointId == null && i.StaticScore == 0).ToList();
				var pointsInsideIsochrone = pointsWithoutParentPoint.Where(i => (bool)isochroneSqlGeography.STContains(i.Point.ToSqlGeography())).ToList();
				if (pointsInsideIsochrone.Count() > 1)
				{
					var positionsInsideIsochrone = pointsInsideIsochrone.Where(x => x.PointId != points.ElementAt(i).PointId).Select(x => (Position)x.Point.Coordinates).ToArray();
					var travelTimesMatrixModel = OsrmAPIHelper.GetTravelTimesMatrix((Position)points.ElementAt(i).Point.Coordinates, positionsInsideIsochrone);
					var durationsList = travelTimesMatrixModel.durations[0].ToList();
					var durationsListSortedWithIndexes = durationsList.Select((x, index) => new KeyValuePair<int, float>(index, x)).OrderBy(x => x.Value).ToList();

					// get route that innerdistance and time is no longer than 30min and x? km
					RouteModel resultRoute;
					double maxInnerDistance = 30 * 1000;
					double maxInnerTime = 15 * 60;
					var pointIds = GetRouteMeetingConditionsAndPointIds(durationsListSortedWithIndexes.Select(x => x.Key).ToList(), pointsInsideIsochrone, maxInnerDistance, maxInnerTime, out resultRoute);

					if (pointIds != null)
					{
						// Create aggregated point
						var aggregatedPoint = GetAggregatedPoint(pointIds, pointsWithoutParentPoint, resultRoute.Distance, resultRoute.Time);
						var addedPoint = repo.Add(aggregatedPoint);
						// TODO: how to get addedPoint ID? for updating child point ParentID field
						
						// Update parentId for child points
						// TODO: is update correct?
						var updatedPoints = GetUpdatedChildPointsWithParentId(pointIds, ref points, addedPoint.PointId);
						foreach(var point in updatedPoints)
						{
							repo.Update(point);
						}
					}

				}

			}

		}

		private static List<long?> GetRouteMeetingConditionsAndPointIds(List<int> indexesOfDurationList, List<DbModel.LocalizationPointDto> pointsInsideIsochrone, double maxInnerDistance, double maxInnerTime, out RouteModel route)
		{
			var sortedPointsByDuration = new List<DbModel.LocalizationPointDto>();
			foreach (var index in indexesOfDurationList)
			{
				sortedPointsByDuration.Add(pointsInsideIsochrone.ElementAt(index));
			}

			bool doesRouteMeetParameters = false;
			route = null;
			while (!doesRouteMeetParameters)
			{
				var routeJson = OsrmAPIHelper.GetOptimalRoute(sortedPointsByDuration.Select(x => (Position)x.Point.Coordinates).ToArray());
				if (routeJson == null)
				{
					route = null;
					return null;
				}
				route = routeJson.ToRouteModel();
				doesRouteMeetParameters = DoesInnerRouteMeetParameters(route, maxInnerDistance, maxInnerTime);
				if (!doesRouteMeetParameters)
				{
					sortedPointsByDuration.RemoveAt(sortedPointsByDuration.Count() - 1);
				}
			}

			var pointIds = sortedPointsByDuration.Select(x => x.PointId).ToList();
			return pointIds;
		}

		private static  bool DoesInnerRouteMeetParameters(RouteModel route, double maxInnerDistance, double maxInnerTime)
		{
			return route.Distance <= maxInnerDistance && route.Time <= maxInnerTime;
		}

		private static  List<LocalizationPointDto> GetUpdatedChildPointsWithParentId(List<long?> pointIds, ref List<DbModel.LocalizationPointDto> points, long? parentId)
		{
			points.Where(x => pointIds.Contains(x.PointId)).ToList().ForEach(x => x.ParentPointId = parentId);
			var pointsWithIds = points.Where(x => pointIds.Contains(x.PointId)).ToList();
			return pointsWithIds;
		}

		private static LocalizationPointDto GetAggregatedPoint(List<long?> pointIds, List<DbModel.LocalizationPointDto> points, double innerDistance, double innerTime)
		{
			var localizationPoint = new LocalizationPointDto()
			{
				Point = new GeoJSON.Net.Geometry.Point(GetPositionForAggregatedPoint(pointIds, points)),
				StaticScore = pointIds.Count(),
				InnerDistance = innerDistance,
				InnerTime = innerTime
			};
			return localizationPoint;
		}

		private static Position GetPositionForAggregatedPoint(List<long?> pointIds, List<DbModel.LocalizationPointDto> points)
		{
			var pointsWithIds = points.Where(x => pointIds.Contains(x.PointId)).ToList();
			var longitude = pointsWithIds.Select(x => x.Point.Coordinates.Longitude).DefaultIfEmpty(0).Average();
			var latitude = pointsWithIds.Select(x => x.Point.Coordinates.Latitude).DefaultIfEmpty(0).Average();

			return new Position(latitude, longitude);
		}

		private static SqlGeography GetCorrectlyOrientedGeography(SqlGeography geography)
		{
			var invertedSqlGeography = geography.ReorientObject();
			if (geography.STArea() > invertedSqlGeography.STArea())
			{
				return invertedSqlGeography;
			}
			return geography;
		}


	}
}

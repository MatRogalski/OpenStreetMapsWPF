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

			InsertDataFromFileToDb(@$"C:\GIT\private\map\polska_1000.csv");
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
			List<LocalizationPointDto> points = repo.GetWithoutAggregated();
			var mappedPointsIds = new HashSet<long>();

			foreach (LocalizationPointDto dbPoint in points)
			{
				if (dbPoint.ParentPointId != null || dbPoint.StaticScore != 0 || mappedPointsIds.Contains(dbPoint.PointId.Value))
					continue;

				Polygon isochrone = MapboxAPIHelper.GetIsochroneAsPolygon((Position)dbPoint.Point.Coordinates, 15);
				SqlGeography isochroneSqlGeography = isochrone.ToSqlGeography().MakeValid();
				isochroneSqlGeography = GetCorrectlyOrientedGeography(isochroneSqlGeography);

				// take points that are not parent or child points in aggregation
				var pointsWithoutParentPoint = points.Where(i => i.ParentPointId == null && i.StaticScore == 0 && !mappedPointsIds.Contains(i.PointId.Value)).ToList();
				var pointsInsideIsochrone = pointsWithoutParentPoint.Where(i => (bool)isochroneSqlGeography.STContains(i.Point.ToSqlGeography())).ToList();
				if (pointsInsideIsochrone.Count > 1)
				{
					Position[] positionsInsideIsochrone = pointsInsideIsochrone.Where(x => x.PointId != dbPoint.PointId).Select(x => (Position)x.Point.Coordinates).ToArray();

					TravelTimesMatrixModel travelTimesMatrixModel = OsrmAPIHelper.GetTravelTimesMatrix((Position)dbPoint.Point.Coordinates, positionsInsideIsochrone);
					var durationsList = travelTimesMatrixModel.durations != null ? travelTimesMatrixModel.durations[0].ToList() : new List<float>();
					var durationsListSortedWithIndexes = durationsList.Select((x, index) => new KeyValuePair<int, float>(index, x)).OrderBy(x => x.Value).ToList();

					// get route that innerdistance and time is no longer than 30min and x? km
					double maxInnerDistance = 30 * 1000;
					double maxInnerTime = 15 * 60;
					List<long?> pointIds = GetRouteMeetingConditionsAndPointIds(durationsListSortedWithIndexes.Select(x => x.Key).ToList(), pointsInsideIsochrone, maxInnerDistance, maxInnerTime, out RouteModel resultRoute);

					if (pointIds != null)
					{
						pointsInsideIsochrone.ForEach(i => mappedPointsIds.Add(i.PointId.Value));

						// Create aggregated point
						LocalizationPointDto aggregatedPoint = GetAggregatedPoint(pointIds, pointsWithoutParentPoint, resultRoute.Distance, resultRoute.Time);
						LocalizationPointDto addedPoint = repo.Add(aggregatedPoint);

						// Update parentId for child points
						List<LocalizationPointDto> updatedPoints = GetUpdatedChildPointsWithParentId(pointIds, points, addedPoint.PointId);
						foreach(LocalizationPointDto point in updatedPoints)
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
			foreach (int index in indexesOfDurationList)
			{
				sortedPointsByDuration.Add(pointsInsideIsochrone.ElementAt(index));
			}

			bool doesRouteMeetParameters = false;
			route = null;
			while (!doesRouteMeetParameters)
			{
				OsrmJsonRouteModel routeJson = OsrmAPIHelper.GetOptimalRoute(sortedPointsByDuration.Select(x => (Position)x.Point.Coordinates).ToArray());
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

		private static bool DoesInnerRouteMeetParameters(RouteModel route, double maxInnerDistance, double maxInnerTime)
		{
			return route.Distance <= maxInnerDistance && route.Time <= maxInnerTime;
		}

		private static List<LocalizationPointDto> GetUpdatedChildPointsWithParentId(List<long?> pointIds, List<DbModel.LocalizationPointDto> points, long? parentId)
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
			double longitude = pointsWithIds.Select(x => x.Point.Coordinates.Longitude).DefaultIfEmpty(0).Average();
			double latitude = pointsWithIds.Select(x => x.Point.Coordinates.Latitude).DefaultIfEmpty(0).Average();

			return new Position(latitude, longitude);
		}

		private static SqlGeography GetCorrectlyOrientedGeography(SqlGeography geography)
		{
			SqlGeography invertedSqlGeography = geography.ReorientObject();
			if (geography.STArea() > invertedSqlGeography.STArea())
			{
				return invertedSqlGeography;
			}
			return geography;
		}


	}
}

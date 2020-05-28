using DbConnector.Repositories;
using DbModel;
using GeoJSON.Net.Contrib.MsSqlSpatial;
using GeoJSON.Net.Geometry;
using Router.APIHelpers;
using Router.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Router
{
	public class Router
	{
		private readonly Position startingPosition;
		private readonly Position endingPositions;
		private readonly double totalAdditionalDistance;
		private readonly double totalAdditionalTime;
		private double totalRouteDistance;
		private double totalRouteTime;
		private readonly ILocalizationPointRepository repo;
		private readonly List<Position> waypoints;
		private RouteModel referenceRoute;
		private RouteModel resultRoute;
		private RouteModel lastDynamicScoreCalculatedRoute;
		private bool doesRouteMeetParameters;
		private bool dynamicScoreNeedsToBeRecalculated;

		private const double AREA_RATIO_THRESHOLD = 0.5;

		public RouteModel ReferenceRoute { get { return referenceRoute; } }

		private Router()
		{
			// TODO: change from mock
			//this.repo = new LocalizationPointRepository();
			this.repo = new MockLocalizationPointRepository();
			this.waypoints = new List<Position>();
			this.doesRouteMeetParameters = true;
			this.dynamicScoreNeedsToBeRecalculated = true;
		}

		public Router(Position startingPoint, Position endingPoint, double additionalDistance, double additionalTime) : this()
		{
			this.startingPosition = startingPoint;
			this.endingPositions = endingPoint;
			this.totalAdditionalDistance = additionalDistance;
			this.totalAdditionalTime = additionalTime;
		}


		public RouteModel GetRoute(bool useAggregatedPoints)
		{
			this.referenceRoute = this.GetRouteBetweenTwoPoints();
			this.totalRouteDistance = this.referenceRoute.Distance + this.totalAdditionalDistance;
			this.totalRouteTime = this.referenceRoute.Time + this.totalAdditionalTime;

			this.resultRoute = this.referenceRoute;
			var availablePoints = (useAggregatedPoints ? this.repo.GetWithAggregated() : this.repo.GetWithoutAggregated()).ToList();
			this.ProcessAvailablePoints(availablePoints, this.totalAdditionalDistance, this.totalAdditionalTime);
			return this.resultRoute;
		}

		private void ProcessAvailablePoints(List<LocalizationPoint> availablePoints, double currentAdditionalDistance, double currentAdditionalTime, int? stepSize = null)
		{
			while (this.doesRouteMeetParameters)
			{
				availablePoints = this.GetPointsDynamicScoreUsingBuffer(availablePoints, currentAdditionalDistance, currentAdditionalTime, countScore: this.dynamicScoreNeedsToBeRecalculated, stepSize);

				if (!availablePoints.Any())
					break;

				LocalizationPoint biggestScorePoint = availablePoints.Aggregate((i1, i2) => i1.DynamicScore > i2.DynamicScore ? i1 : i2);

				availablePoints.Remove(biggestScorePoint);

				if (biggestScorePoint.StaticScore == 0)
				{
					this.AddSimplePoint(biggestScorePoint, ref currentAdditionalDistance, ref currentAdditionalTime);
				}
				else
				{
					this.AddAggregatedPoint(biggestScorePoint, ref currentAdditionalDistance, ref currentAdditionalTime);
				}
			}
		}

		private void AddAggregatedPoint(LocalizationPoint biggestScorePoint, ref double currentAdditionalDistance, ref double currentAdditionalTime)
		{
			List<LocalizationPoint> innerPoints = this.repo.GetByParentId(biggestScorePoint.PointId.Value);
			var tempWaypoints = this.waypoints.Select(i => i).ToList();
			tempWaypoints.AddRange(innerPoints.Select(i => (Position)i.Point.Coordinates));

			RouteModel newRoute = this.GetRouteBetweenTwoPoints(tempWaypoints);
			this.doesRouteMeetParameters = this.DoesRouteMeetParameters(newRoute, currentAdditionalDistance, currentAdditionalTime);
			if (this.doesRouteMeetParameters)
			{
				this.dynamicScoreNeedsToBeRecalculated = this.DoesDynamicScoreNeedToBeRecalculated(this.lastDynamicScoreCalculatedRoute, newRoute, (Position)biggestScorePoint.Point.Coordinates);
				this.UpdateResultRoute(newRoute, out currentAdditionalDistance, out currentAdditionalTime);
			}
			else
			{
				this.ProcessAvailablePoints(innerPoints, currentAdditionalDistance, currentAdditionalTime, (int)biggestScorePoint.InnerDistance / 10);
			}
		}


		private void AddSimplePoint(LocalizationPoint biggestScorePoint, ref double currentAdditionalDistance, ref double currentAdditionalTime)
		{
			this.waypoints.Add((Position)biggestScorePoint.Point.Coordinates);
			RouteModel newRoute = this.GetRouteBetweenTwoPoints(this.waypoints);

			this.doesRouteMeetParameters = this.DoesRouteMeetParameters(newRoute, this.totalAdditionalDistance, this.totalAdditionalTime);
			if (this.doesRouteMeetParameters)
			{
				this.dynamicScoreNeedsToBeRecalculated = this.DoesDynamicScoreNeedToBeRecalculated(this.lastDynamicScoreCalculatedRoute, newRoute, (Position)biggestScorePoint.Point.Coordinates);
				this.UpdateResultRoute(newRoute, out currentAdditionalDistance, out currentAdditionalTime);
			}
		}

		private void UpdateResultRoute(RouteModel newRoute, out double currentAdditionalDistance, out double currentAdditionalTime)
		{
			double currentDistance = newRoute.Distance;
			double currentTime = newRoute.Time;

			currentAdditionalDistance = this.totalRouteDistance - currentDistance;
			currentAdditionalTime = this.totalRouteTime - currentTime;
			this.resultRoute = newRoute;
		}


		private List<LocalizationPoint> GetPointsDynamicScoreUsingBuffer(List<LocalizationPoint> points, double additionalDistance, double additionalTime, bool countScore = true, int? stepSize = null)
		{
			var result = new List<LocalizationPoint>();
			double halfOfAdditionalDistance = additionalDistance / 2;
			Microsoft.SqlServer.Types.SqlGeography routeSqlGeography = this.resultRoute.MultiPointGeoJsonNet.ToSqlGeography();

			int bufferSize = (int)halfOfAdditionalDistance;

			int step = stepSize ?? (int)halfOfAdditionalDistance / 10;
			bool isFirstRun = true;

			//TODO: include additional time to counting dynamic score/buffer

			//TODO: think about counting score for inner points algorithm and its computational complexity 

			while (bufferSize > 0)
			{
				Microsoft.SqlServer.Types.SqlGeography bufferSqlGeography = routeSqlGeography.STBuffer(bufferSize);

				if (isFirstRun)
				{
					result = points.Where(i => (bool)bufferSqlGeography.STContains(i.Point.ToSqlGeography())).ToList();

					if (result.Any(i => i.DynamicScore > 0))
						if (!countScore)
							break;

					// can not work because reference type
					this.lastDynamicScoreCalculatedRoute = this.resultRoute;
					result.ForEach(i => i.DynamicScore = (i.StaticScore ?? 0) + 1);
					isFirstRun = false;
					if (!countScore)
						break;
				}
				else
				{
					points.Where(i => (bool)bufferSqlGeography.STContains(i.Point.ToSqlGeography())).ToList().ForEach(i => ++i.DynamicScore);
				}

				bufferSize -= step;
			}

			return result;
		}

		private bool AllPointsHasSameParent(List<LocalizationPoint> points)
		{
			if (points.Any(i => i.ParentPointId == null))
			{
				return false;
			}
			else
			{
				long? firstParent = points.FirstOrDefault().ParentPointId;
				return points.All(i => i.ParentPointId == firstParent);
			}
		}

		private bool DoesDynamicScoreNeedToBeRecalculated(RouteModel oldRoute, RouteModel newRoute, Position addedPoint)
		{
			var oldRouteGeography = oldRoute.MultiPointGeoJsonNet.ToSqlGeography();
			var newRouteGeography = newRoute.MultiPointGeoJsonNet.ToSqlGeography();
			var bufferOld = oldRouteGeography.STBuffer(1000);
			var bufferNew = newRouteGeography.STBuffer(1000);
			var difference = bufferNew.STDifference(bufferOld);

			double diffArea = (double)difference.STArea();
			double oldArea = (double)bufferOld.STArea();
			double newArea = (double)bufferNew.STArea();

			// TODO: think about const and if ratio calculation is correct (no other way = diff/new)
			double areaRatio = diffArea / oldArea;

			return areaRatio > AREA_RATIO_THRESHOLD;
		}

		private bool DoesRouteMeetParameters(RouteModel route, double additionalDistance, double additionalTime)
		{
			if (route.Distance > this.totalRouteDistance)
				return false;
			if (route.Time > this.totalRouteTime)
				return false;

			return true;
		}

		private RouteModel GetRouteBetweenTwoPoints()
		{
			// TODO: GetSimpleRoute or GetOptimalRoute? - probably Simple
			var routeJson = OsrmAPIHelper.GetSimpleRoute(startingPosition, endingPositions);
			return routeJson.ToRouteModel();
		}


		private RouteModel GetRouteBetweenTwoPoints(List<Position> waypoints)
		{
			// TODO: GetSimpleRoute or GetOptimalRoute? - probalby Optimal
			var routeJson = OsrmAPIHelper.GetOptimalRoute(this.startingPosition, this.endingPositions, waypoints.ToArray());
			return routeJson.ToRouteModel();
		}
	}
}

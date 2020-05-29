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
	public class Router : BaseRouter
	{
		private RouteModel lastDynamicScoreCalculatedRoute;
		private bool dynamicScoreNeedsToBeRecalculated;

		private const double AREA_RATIO_THRESHOLD = 0.5;


		public Router(Position startingPoint, Position endingPoint, double additionalDistance, double additionalTime) : base(startingPoint, endingPoint, additionalDistance, additionalTime)
		{
			this.doesRouteMeetParameters = true;
			this.dynamicScoreNeedsToBeRecalculated = true;
		}

		protected override void ProcessAvailablePoints(List<LocalizationPointDto> availablePoints, double currentAdditionalDistance, double currentAdditionalTime, int? stepSize = null)
		{
			// TODO: think about not processsing multiple times points that are aggregated 
			while (this.doesRouteMeetParameters)
			{
				availablePoints = this.GetPointsDynamicScoreUsingBuffer(availablePoints, currentAdditionalDistance, currentAdditionalTime, countScore: this.dynamicScoreNeedsToBeRecalculated, stepSize);

				if (!availablePoints.Any())
					break;

				LocalizationPointDto biggestScorePoint = availablePoints.Aggregate((i1, i2) => i1.DynamicScore > i2.DynamicScore ? i1 : i2);

				availablePoints.Remove(biggestScorePoint);
				// TODO: think about if biggestScorePoint = aggregated -> remove all child points

				if (biggestScorePoint.StaticScore == 0)
				{
					this.AddSimplePoint(biggestScorePoint, ref currentAdditionalDistance, ref currentAdditionalTime);
				}
				else
				{
					this.AddAggregatedPoint(biggestScorePoint, ref currentAdditionalDistance, ref currentAdditionalTime);
					// TODO: jesli punkt zaagregowany jest rozbity na pomniejsze punkty to pozwala na dalsze przetwarzanie zwyklych punktow
					// ale za to jest szansa ze wejdziemy w petle nieskoczona - gdyby zostaly tylko punkty zaagregowane
					// dlatego moze to usunac
					this.doesRouteMeetParameters = true;
				}
			}
		}

		private void AddAggregatedPoint(LocalizationPointDto biggestScorePoint, ref double currentAdditionalDistance, ref double currentAdditionalTime)
		{
			List<LocalizationPointDto> innerPoints = this.repo.GetByParentId(biggestScorePoint.PointId.Value);
			var tempWaypoints = this.waypoints.Select(i => i).ToList();
			tempWaypoints.AddRange(innerPoints.Select(i => (Position)i.Point.Coordinates));

			RouteModel newRoute = this.GetRouteBetweenTwoPoints(tempWaypoints);
			this.doesRouteMeetParameters = this.DoesRouteMeetParameters(newRoute, currentAdditionalDistance, currentAdditionalTime);
			if (this.doesRouteMeetParameters)
			{
				this.waypoints.Clear();
				this.waypoints.AddRange(tempWaypoints);
				this.dynamicScoreNeedsToBeRecalculated = this.DoesDynamicScoreNeedToBeRecalculated(this.lastDynamicScoreCalculatedRoute, newRoute, (Position)biggestScorePoint.Point.Coordinates);
				this.UpdateResultRoute(newRoute, out currentAdditionalDistance, out currentAdditionalTime);
			}
			else
			{
				this.doesRouteMeetParameters = true;
				this.ProcessAvailablePoints(innerPoints, currentAdditionalDistance, currentAdditionalTime, (int)biggestScorePoint.InnerDistance / 10);
			}
		}


		private void AddSimplePoint(LocalizationPointDto biggestScorePoint, ref double currentAdditionalDistance, ref double currentAdditionalTime)
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

			currentAdditionalDistance = this.maxAllowedRouteDistance - currentDistance;
			currentAdditionalTime = this.maxAllowedRouteTime - currentTime;
			this.resultRoute = newRoute;
		}


		private List<LocalizationPointDto> GetPointsDynamicScoreUsingBuffer(List<LocalizationPointDto> points, double additionalDistance, double additionalTime, bool countScore = true, int? stepSize = null)
		{
			var result = new List<LocalizationPointDto>();
			double halfOfAdditionalDistance = additionalDistance / 2;
			Microsoft.SqlServer.Types.SqlGeography routeSqlGeography = this.resultRoute.MultiPointGeoJsonNet.ToSqlGeography();

			int bufferSize = (int)halfOfAdditionalDistance;

			int step = stepSize ?? (int)halfOfAdditionalDistance / 10;
			bool isFirstRun = true;

			//TODO: include additional time to counting dynamic score/buffer

			//TODO: think about counting score for inner points algorithm and its computational complexity 

			while (bufferSize > 0)
			{
				if (isFirstRun)
				{
					if (points.Any(i => i.DynamicScore > 0))
						if (!countScore)
							return points;

					Microsoft.SqlServer.Types.SqlGeography bufferSqlGeography = routeSqlGeography.STBuffer(bufferSize);
					result = points.Where(i => (bool)bufferSqlGeography.STContains(i.Point.ToSqlGeography())).ToList();

					// may not work because reference type - seems to work
					this.lastDynamicScoreCalculatedRoute = this.resultRoute;
					result.ForEach(i => i.DynamicScore = (i.StaticScore ?? 0) + 1);
					isFirstRun = false;
					if (!countScore)
						break;
				}
				else
				{
					Microsoft.SqlServer.Types.SqlGeography bufferSqlGeography = routeSqlGeography.STBuffer(bufferSize);
					points.Where(i => (bool)bufferSqlGeography.STContains(i.Point.ToSqlGeography())).ToList().ForEach(i => ++i.DynamicScore);
				}

				bufferSize -= step;
			}

			return result;
		}

		private bool AllPointsHasSameParent(List<LocalizationPointDto> points)
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



	}
}

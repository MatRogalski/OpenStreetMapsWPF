using DbConnector.Repositories;
using DbModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Router
{
	public class Router
	{
		private readonly Point startingPoint;
		private readonly Point endingPoint;
		private readonly double totalAdditionalDistance;
		private readonly double totalAdditionalTime;
		private readonly LocalizationPointRepository repo;
		private readonly List<Point> waypoints;
		private MultiPoint referenceRoute;
		private MultiPoint resultRoute;
		private bool doesRouteMeetParameters;
		private bool dynamicScoreNeedsToBeRecalculated;


		private Router()
		{
			this.repo = new LocalizationPointRepository();
			this.waypoints = new List<Point>();
			this.doesRouteMeetParameters = true;
			this.dynamicScoreNeedsToBeRecalculated = true;
		}

		public Router(Point startingPoint, Point endingPoint, double additionalDistance, double additionalTime) : this()
		{
			this.startingPoint = startingPoint;
			this.endingPoint = endingPoint;
			this.totalAdditionalDistance = additionalDistance;
			this.totalAdditionalTime = additionalTime;
		}


		public MultiPoint GetRoute(bool useAggregatedPoints)
		{
			this.referenceRoute = this.GetRouteBetweenTwoPoints();
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
			tempWaypoints.AddRange(innerPoints.Select(i => i.Coordinate));

			MultiPoint newRoute = this.GetRouteBetweenTwoPoints(tempWaypoints);
			this.doesRouteMeetParameters = this.DoesRouteMeetParameters(newRoute, currentAdditionalDistance, currentAdditionalTime);
			if (this.doesRouteMeetParameters)
			{
				this.dynamicScoreNeedsToBeRecalculated = this.DoesDynamicScoreNeedToBeRecalculated(this.resultRoute, newRoute, biggestScorePoint.Coordinate);
				this.UpdateResultRoute(newRoute, out currentAdditionalDistance, out currentAdditionalTime);
			}
			else
			{
				this.ProcessAvailablePoints(innerPoints, currentAdditionalDistance, currentAdditionalTime, (int)biggestScorePoint.InnerDistance / 10);
			}
		}


		private void AddSimplePoint(LocalizationPoint biggestScorePoint, ref double currentAdditionalDistance, ref double currentAdditionalTime)
		{
			this.waypoints.Add(biggestScorePoint.Coordinate);
			MultiPoint newRoute = this.GetRouteBetweenTwoPoints(this.waypoints);

			this.doesRouteMeetParameters = this.DoesRouteMeetParameters(newRoute, this.totalAdditionalDistance, this.totalAdditionalTime);
			if (this.doesRouteMeetParameters)
			{
				this.dynamicScoreNeedsToBeRecalculated = this.DoesDynamicScoreNeedToBeRecalculated(this.resultRoute, newRoute, biggestScorePoint.Coordinate);
				this.UpdateResultRoute(newRoute, out currentAdditionalDistance, out currentAdditionalTime);
			}
		}

		private void UpdateResultRoute(MultiPoint newRoute, out double currentAdditionalDistance, out double currentAdditionalTime)
		{
			//TODO: current additional distance and current additional time needs to be updated every result route update, this is only mock
			currentAdditionalDistance = this.totalAdditionalDistance * new Random().NextDouble();
			currentAdditionalTime = this.totalAdditionalTime * new Random().NextDouble();
			this.resultRoute = newRoute;
		}


		private List<LocalizationPoint> GetPointsDynamicScoreUsingBuffer(List<LocalizationPoint> points, double additionalDistance, double additionalTime, bool countScore = true, int? stepSize = null)
		{
			var result = new List<LocalizationPoint>();
			double halfOfAdditionalDistance = additionalDistance / 2;
			var buffer = new BufferOp(this.resultRoute);

			int bufferSize = (int)halfOfAdditionalDistance;

			int step = stepSize ?? (int)halfOfAdditionalDistance / 10;
			bool isFirstRun = true;

			//TODO: include additional time to counting dynamic score/buffer

			//TODO: think about counting score for inner points algorithm and its computational complexity 

			while (bufferSize > 0)
			{
				Geometry geometry = buffer.GetResultGeometry(bufferSize);

				if (isFirstRun)
				{
					result = points.Where(i => geometry.Contains(i.Coordinate)).ToList();
					result.ForEach(i => i.DynamicScore = (i.StaticScore ?? 0) + 1);
					isFirstRun = false;
					if (!countScore)
						break;
				}
				else
				{
					points.Where(i => geometry.Contains(i.Coordinate)).ToList().ForEach(i => ++i.DynamicScore);
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

		private bool DoesDynamicScoreNeedToBeRecalculated(MultiPoint oldRoute, MultiPoint newRoute, Point addedPoint)
		{
			//TODO: needs implementation

			return new Random().Next(20) >= 10;
		}

		private bool DoesRouteMeetParameters(MultiPoint route, double additionalDistance, double additionalTime)
		{
			//TODO: needs implementation

			return new Random().Next(20) >= 2;
		}

		private MultiPoint GetRouteBetweenTwoPoints()
		{
			//TODO: needs implementation

			return new MultiPoint(new Point[] { this.startingPoint, this.endingPoint });
		}


		private MultiPoint GetRouteBetweenTwoPoints(List<Point> waypoints)
		{
			//TODO: needs implementation

			var points = new List<Point>();
			points.Add(this.startingPoint);
			points.AddRange(waypoints);
			points.Add(this.endingPoint);

			return new MultiPoint(points.ToArray());
		}
	}
}

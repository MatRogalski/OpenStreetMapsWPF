using DbModel;
using GeoJSON.Net.Geometry;
using Router.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Router
{
    public class RouterOptimalBruteForce : BaseRouter
    {
		public RouterOptimalBruteForce(Position startingPoint, Position endingPoint, double additionalDistance, double additionalTime) : base(startingPoint, endingPoint, additionalDistance, additionalTime)
		{
		}

		protected override void ProcessAvailablePoints(List<LocalizationPointDto> availablePoints, double currentAdditionalDistance, double currentAdditionalTime, int? stepSize = null)
        {
			var allPointsCombinations = GetPowerSet(availablePoints);

			List<List<Position>> allPositionsCombinations = new List<List<Position>>();
			foreach (var pointCombination in allPointsCombinations)
			{
				List<Position> positionCombination = new List<Position>();
				foreach (var point in pointCombination)
				{
					Position position = (Position)point.Point.Coordinates;
					positionCombination.Add(position);
				}
				if(positionCombination.Any())
					allPositionsCombinations.Add(positionCombination);
			}

			List<RouteModel> allPossibleRoutes = new List<RouteModel>();
			foreach (var positionCombination in allPositionsCombinations)
			{
				RouteModel route = GetRouteBetweenTwoPoints(positionCombination);
				if(DoesRouteMeetParameters(route, currentAdditionalDistance, currentAdditionalTime))
					allPossibleRoutes.Add(route);
			}

			//var routesThatMeetParameters = allPossibleRoutes.Where(i => i.Distance <= maxAllowedRouteDistance && i.Time <= maxAllowedRouteTime);
			int maxNumberOfWaypoints = allPossibleRoutes.Max(i => i.Waypoints.Length);
			var routesWithMaxWaypoints = allPossibleRoutes.Where(i => i.Waypoints.Length == maxNumberOfWaypoints);
			var routesOrderedByDistanceAndTime = routesWithMaxWaypoints.OrderBy(i => i.Time).ThenBy(i => i.Distance);

			this.resultRoute = routesOrderedByDistanceAndTime.Take(1).SingleOrDefault();
        }

		private IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list)
		{
			return from m in Enumerable.Range(0, 1 << list.Count)
				   select
					   from i in Enumerable.Range(0, list.Count)
					   where (m & (1 << i)) != 0
					   select list[i];
		}

	}
}

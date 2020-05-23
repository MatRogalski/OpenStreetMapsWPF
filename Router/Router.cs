using MapControl;
using System;
using ViewModel;

namespace Router
{
	public class Router
	{
		public void GetRouteWithoutAgregatedPoints(Location startingPoint, Location endingPoint, double additionalDistance, double additionalTime)
		{
			 
		}


		protected Polyline GetRouteBetweenTwoPoints(Location startingPoint, Location endingPoint)
		{
			//TODO: needs implementation
			var result = new Polyline();
			result.Locations = new LocationCollection();

			return result;
		}
	}
}

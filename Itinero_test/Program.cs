using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itinero_test
{
	class Program
	{
		static void Main(string[] args)
		{
			//ProcessDbToGetOnlyCarRoutes();
			RouterDb routerDb = null;
			using (var stream = new FileInfo(@"C:\GIT\private\map\poland.routerdb").OpenRead())
			{
				routerDb = RouterDb.Deserialize(stream);
			}
			var router = new Router(routerDb);

			//get a profile
			var profile = Vehicle.Car.Fastest(); //the default OSM car profile


			routerDb.AddContracted(profile);

			Coordinate from = new Coordinate(52.255135f, 20.982435f);
			Coordinate to = new Coordinate(53.111618f, 20.383173f);

			//create a routerpoint from a location
			//snaps the given location to the nearest routable edge
			var start = router.TryResolve(profile, from, 200).Value;
			var end = router.TryResolve(profile, to, 200).Value;

			List<RouterPoint> points = new List<RouterPoint>()
			{
				router.TryResolve(profile, from, 200).Value,
				router.TryResolve(profile, to, 200).Value
			};



			//calculate a route
			//var route = router.Calculate(profile, start, end);
			var stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
			var route = router.TryCalculate(profile, points.ToArray());
			stopwatch.Stop();
			Console.WriteLine($"Elapsed: {stopwatch.Elapsed.Minutes} min {stopwatch.Elapsed.Seconds} sec");
			if (!route.IsError)
			{
				using (var writer = new StreamWriter("route.geojson"))
				{
					route.Value.WriteGeoJson(writer);
				}
			}
		}

		private static void ProcessDbToGetOnlyCarRoutes()
		{
			var routerDb = new RouterDb();
			using (var stream = new FileInfo(@"C:\GIT\private\map\poland-latest.osm.pbf").OpenRead())
			{
				// create the network for cars.
				routerDb.LoadOsmData(stream, Vehicle.Car);
			}

			// write the routerDb to disk
			using (var stream = new FileInfo(@"C:\GIT\private\map\poland.routerdb").Open(FileMode.Create))
			{
				routerDb.Serialize(stream);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Itinero;
using System.IO;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;
using Itinero.LocalGeo;
using System.Globalization;
using MapControl;
using ViewModel;
using Router.Model;
using Newtonsoft.Json;
using Router.Utils;
using Router.APIHelpers;
using NetTopologySuite.Operation.Buffer;
using GeoJSON.Net.Contrib.MsSqlSpatial;
using DbConnector.Repositories;
using GeoJSON.Net.Geometry;
using Microsoft.SqlServer.Types;
using DbModel;

namespace MapDisplayApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			ImageLoader.HttpClient.DefaultRequestHeaders.Add("User-Agent", "XAML Map Control Test Application");
			TileImageLoader.Cache = new MapControl.Caching.ImageFileCache(TileImageLoader.DefaultCacheFolder);
			//TileImageLoader.Cache = new MapControl.Caching.FileDbCache(TileImageLoader.DefaultCacheFolder);
			//TileImageLoader.Cache = new MapControl.Caching.SQLiteCache(TileImageLoader.DefaultCacheFolder);
			//TileImageLoader.Cache = null;

			this.InitializeComponent();



			//InsertAggregatedPointsToDb();

			//OsrmTravelTimeTableUsage();
			//MapboxAPIUsage();

			//ViewModel.Polyline polyline = GetMapControlPolyLineFromOsrmApi();
			//this.AddPolylineToMap(polyline);


			//var lines = GetDifferencePolyline();
			//foreach(var line in lines)
			//{
			//	this.AddPolylineToMap(line);
			//}

		}

		private void InsertAggregatedPointsToDb()
		{
			var repo = new MockLocalizationPointRepositoryMazowieckie();
			var pointItems = GetPointsItemsFromRepo(repo);
			foreach(var pointItem in pointItems)
			{
				(this.DataContext as MapViewModel).Pushpins.Add(pointItem);
			}

			var points = repo.GetWithoutAggregated();

			for (int i = 0; i < points.Count(); i++)
			{
				if (points.ElementAt(i).ParentPointId != null || points.ElementAt(i).StaticScore != 0)
					continue;

				var isochrone = MapboxAPIHelper.GetIsochroneAsPolygon((Position)points.ElementAt(i).Point.Coordinates, 15);
				SqlGeography isochroneSqlGeography = isochrone.ToSqlGeography().MakeValid();
				isochroneSqlGeography = GetCorrectlyOrientedGeography(isochroneSqlGeography);
				this.AddPolylineToMap(GetFromSqlGeography(isochroneSqlGeography));

				// take points that are not parent or child points in aggregation
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

					if(pointIds != null)
					{
						// Create aggregated point
						var aggregatedPoint = GetAggregatedPoint(pointIds, pointsWithoutParentPoint, resultRoute.Distance, resultRoute.Time);
						points.Add(aggregatedPoint);
						// Update parentId for child points
						var updatedPoints = GetUpdatedChildPointsWithParentId(pointIds, ref points, aggregatedPoint.PointId);
					}

				}

			}

		}

		private List<long?> GetRouteMeetingConditionsAndPointIds(List<int> indexesOfDurationList, List<DbModel.LocalizationPointDto> pointsInsideIsochrone, double maxInnerDistance, double maxInnerTime, out RouteModel route)
		{
			var sortedPointsByDuration = new List<DbModel.LocalizationPointDto>();
			foreach(var index in indexesOfDurationList)
			{
				sortedPointsByDuration.Add(pointsInsideIsochrone.ElementAt(index));
			}

			bool doesRouteMeetParameters = false;
			route = null;
			while (!doesRouteMeetParameters)
			{
				var routeJson = OsrmAPIHelper.GetOptimalRoute(sortedPointsByDuration.Select(x => (Position)x.Point.Coordinates).ToArray());
				if(routeJson == null)
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

		private bool DoesInnerRouteMeetParameters(RouteModel route, double maxInnerDistance, double maxInnerTime)
		{
			return route.Distance <= maxInnerDistance && route.Time <= maxInnerTime;
		}

		private List<LocalizationPointDto> GetUpdatedChildPointsWithParentId(List<long?> pointIds, ref List<DbModel.LocalizationPointDto> points, long? parentId) 
		{
			points.Where(x => pointIds.Contains(x.PointId)).ToList().ForEach(x => x.ParentPointId = parentId);
			var pointsWithIds = points.Where(x => pointIds.Contains(x.PointId)).ToList();
			return pointsWithIds;
		}

		private LocalizationPointDto GetAggregatedPoint(List<long?> pointIds, List<DbModel.LocalizationPointDto> points, double innerDistance, double innerTime)
		{
			var localizationPoint = new LocalizationPointDto()
			{
				PointId = 13,
				Point = new GeoJSON.Net.Geometry.Point(GetPositionForAggregatedPoint(pointIds,points)),
				StaticScore = pointIds.Count(),
				InnerDistance = innerDistance,
				InnerTime = innerTime
			};
			return localizationPoint;
		}

		private Position GetPositionForAggregatedPoint(List<long?> pointIds, List<DbModel.LocalizationPointDto> points)
		{
			var pointsWithIds = points.Where(x => pointIds.Contains(x.PointId)).ToList();
			var longitude = pointsWithIds.Select(x => x.Point.Coordinates.Longitude).DefaultIfEmpty(0).Average();
			var latitude = pointsWithIds.Select(x => x.Point.Coordinates.Latitude).DefaultIfEmpty(0).Average();

			return new Position(latitude, longitude);
		}

		private SqlGeography GetCorrectlyOrientedGeography(SqlGeography geography)
		{
			var invertedSqlGeography = geography.ReorientObject();
			if (geography.STArea() > invertedSqlGeography.STArea())
			{
				return invertedSqlGeography;
			}
			return geography;
		}




		private List<PointItem> GetPointsItemsFromRepo(ILocalizationPointRepository repo)
		{
			var result = new List<PointItem>();
			var points = repo.GetWithoutAggregated();

			foreach(var point in points)
			{
				PointItem pointItem = new PointItem()
				{
					Name = "X",
					Location = new Location(point.Point.Coordinates.Latitude, point.Point.Coordinates.Longitude)
				};
				result.Add(pointItem);
			}

			return result;
		}

		private List<ViewModel.Polyline> GetDifferencePolyline()
		{
			GeoJSON.Net.Geometry.Position warszawa = new GeoJSON.Net.Geometry.Position(52.229941, 21.011105);
			GeoJSON.Net.Geometry.Position mlawa = new GeoJSON.Net.Geometry.Position(53.112154, 20.382377);
			GeoJSON.Net.Geometry.Position plock = new GeoJSON.Net.Geometry.Position(52.546665, 19.687432);
			OsrmJsonRouteModel parsed = OsrmAPIHelper.GetOptimalRoute(warszawa, mlawa);
			var route = parsed.ToRouteModel();

			var jsonNew = OsrmAPIHelper.GetOptimalRoute(warszawa, mlawa, plock);
			var newRoute = jsonNew.ToRouteModel();

			//var oldRouteGMultiLineString = route.MultiLineString.ToSqlGeography();
			//var newRouteGMultiLineString = newRoute.MultiLineString.ToSqlGeography();
			//var difference = newRouteGMultiLineString.STDifference(oldRouteGMultiLineString);


			//var oldRouteG = route.LineString.ToSqlGeography();
			//var newRouteG = newRoute.LineString.ToSqlGeography().MakeValidIfInvalid();
			//string isValid = newRouteG.IsValidDetailed();
			//var difference = newRouteG.STDifference(oldRouteG);


			Microsoft.SqlServer.Types.SqlGeography oldRouteGeography = route.MultiPointGeoJsonNet.ToSqlGeography();
			Microsoft.SqlServer.Types.SqlGeography newRouteGeography = newRoute.MultiPointGeoJsonNet.ToSqlGeography();
			Microsoft.SqlServer.Types.SqlGeography bufferOld = oldRouteGeography.STBuffer(1000);
			Microsoft.SqlServer.Types.SqlGeography bufferNew = newRouteGeography.STBuffer(1000);
			var difference = bufferNew.STDifference(bufferOld);


			double diffArea = (double)difference.STArea();
			double oldArea = (double)bufferOld.STArea();
			double newArea = (double)bufferNew.STArea();

			double howDifferent = diffArea / oldArea;



			List<ViewModel.Polyline> lines = new List<ViewModel.Polyline>();
			//lines.Add(GetFromSqlGeography(oldRouteGeography));
			//lines.Add(GetFromSqlGeography(newRouteGeography));
			//lines.Add(GetFromSqlGeography(difference));
			//lines.Add(GetFromSqlGeography(bufferNew));
			//lines.Add(GetFromSqlGeography(bufferOld));
			//lines.Add(GetFromSqlGeography(oldRouteG));
			//lines.Add(GetFromSqlGeography(newRouteG));
			//lines.Add(GetFromSqlGeography(difference));
			//lines.Add(GetFromSqlGeography(newRouteGMultiLineString));
			//lines.Add(GetFromSqlGeography(oldRouteGMultiLineString));

			var polylines = GetFromSqlGeographies(difference);
			foreach(var line in polylines)
			{
				lines.Add(line);
			}

			return lines;
		}

		private List<ViewModel.Polyline> GetBufferPolyline()
		{
			GeoJSON.Net.Geometry.Position pos1 = new GeoJSON.Net.Geometry.Position(52.218703, 21.005044);
			GeoJSON.Net.Geometry.Position pos2 = new GeoJSON.Net.Geometry.Position(54.182346, 16.189145);
			OsrmJsonRouteModel parsed = OsrmAPIHelper.GetOptimalRoute(pos1, pos2);
			var route = parsed.ToRouteModel();

			Microsoft.SqlServer.Types.SqlGeography sqlGeography = route.MultiPointGeoJsonNet.ToSqlGeography();


			Microsoft.SqlServer.Types.SqlGeography buffer = sqlGeography.STBuffer(100000);


			List<ViewModel.Polyline> lines = new List<ViewModel.Polyline>();
			lines.Add(GetFromSqlGeography(sqlGeography));
			lines.Add(GetFromSqlGeography(buffer));

			return lines;
		}

		private ViewModel.Polyline GetFromSqlGeography(Microsoft.SqlServer.Types.SqlGeography multiPoint)
		{
			var result = new ViewModel.Polyline()
			{
				Locations = new LocationCollection()
			};
			
			for (int i = 1; i <= multiPoint.STNumPoints(); i++)
			{
				Microsoft.SqlServer.Types.SqlGeography point = multiPoint.STPointN(i);
				result.Locations.Add(new Location((double)point.Lat, (double)point.Long));
			}
			return result;
		}

		private List<ViewModel.Polyline> GetFromSqlGeographies(Microsoft.SqlServer.Types.SqlGeography sqlGeography)
		{
			var result = new List<ViewModel.Polyline>();

			for(int i=1;i<= (int)sqlGeography.STNumGeometries(); i++) 
			{
				Microsoft.SqlServer.Types.SqlGeography geography = sqlGeography.STGeometryN(i);
				var polyline = new ViewModel.Polyline()
				{
					Locations = new LocationCollection()
				};

				for (int j = 1; j <= geography.STNumPoints(); j++)
				{
					Microsoft.SqlServer.Types.SqlGeography point = geography.STPointN(j);
					polyline.Locations.Add(new Location((double)point.Lat, (double)point.Long));
				}
				result.Add(polyline);

			}

			return result;
		}

		private ViewModel.Polyline GetFromSqlGeometry(Microsoft.SqlServer.Types.SqlGeometry multiPoint)
		{
			var result = new ViewModel.Polyline()
			{
				Locations = new LocationCollection()
			};

			for (int i = 1; i <= multiPoint.STNumPoints(); i++)
			{
				Microsoft.SqlServer.Types.SqlGeometry point = multiPoint.STPointN(i);
				result.Locations.Add(new Location((double)point.STY, (double)point.STX));
			}
			return result;
		}


		private static void MapboxAPIUsage()
		{
			GeoJSON.Net.Geometry.Position mlawa = new GeoJSON.Net.Geometry.Position(53.112128, 20.383661);
			var polygon = MapboxAPIHelper.GetIsochroneAsPolygon(mlawa, 10);

			GeoJSON.Net.Geometry.Position positionInside = new GeoJSON.Net.Geometry.Position(53.125982, 20.358108);
			GeoJSON.Net.Geometry.Point pointInside = new GeoJSON.Net.Geometry.Point(positionInside);
			GeoJSON.Net.Geometry.Position positionOutside = new GeoJSON.Net.Geometry.Position(53.155378, 20.363038);
			GeoJSON.Net.Geometry.Point pointOutside = new GeoJSON.Net.Geometry.Point(positionOutside);

			bool inside = GeometryUtils.CheckIfPointIsInsidePolygon(polygon, pointInside);
			bool outside = GeometryUtils.CheckIfPointIsInsidePolygon(polygon, pointOutside);
		}

		private static void OsrmTravelTimeTableUsage()
		{
			GeoJSON.Net.Geometry.Position mlawa = new GeoJSON.Net.Geometry.Position(53.112128, 20.383661);
			GeoJSON.Net.Geometry.Position positionInside = new GeoJSON.Net.Geometry.Position(53.125982, 20.358108);
			GeoJSON.Net.Geometry.Position positionOutside = new GeoJSON.Net.Geometry.Position(53.155378, 20.363038);

			OsrmAPIHelper.GetTravelTimesMatrix(mlawa, positionInside, positionOutside);
			MapboxAPIHelper.GetTravelTimesMatrix(mlawa, positionInside, positionOutside);
		}

		private static GeoJSON.Net.Geometry.Position[] GetCoordinatesFromWarszawaToMlawa()
		{
			GeoJSON.Net.Geometry.Position mlawa = new GeoJSON.Net.Geometry.Position(53.112128, 20.383661);
			GeoJSON.Net.Geometry.Position warszawa = new GeoJSON.Net.Geometry.Position(52.230320, 21.011132);
			GeoJSON.Net.Geometry.Position szczecin = new GeoJSON.Net.Geometry.Position(53.421684, 14.561405);
			GeoJSON.Net.Geometry.Position[] coordinates = new GeoJSON.Net.Geometry.Position[3];
			coordinates[0] = warszawa;
			coordinates[1] = mlawa;
			coordinates[2] = szczecin;
			return coordinates;
		}

		private static GeoJSON.Net.Geometry.Position[] GetManyCoordinatesForNormalRoute()
		{
			List<GeoJSON.Net.Geometry.Position> coordinates = new List<GeoJSON.Net.Geometry.Position>();

			GeoJSON.Net.Geometry.Position warszawa = new GeoJSON.Net.Geometry.Position(52.230320, 21.011132);	
			GeoJSON.Net.Geometry.Position bydgoszcz = new GeoJSON.Net.Geometry.Position(53.114519, 18.008936);
			GeoJSON.Net.Geometry.Position lodz = new GeoJSON.Net.Geometry.Position(51.785124, 19.462234);
			GeoJSON.Net.Geometry.Position poznan = new GeoJSON.Net.Geometry.Position(52.398515, 16.938702);
			GeoJSON.Net.Geometry.Position mlawa = new GeoJSON.Net.Geometry.Position(53.112128, 20.383661);
			GeoJSON.Net.Geometry.Position szczecin = new GeoJSON.Net.Geometry.Position(53.421684, 14.561405);

			coordinates.Add(warszawa);
			coordinates.Add(bydgoszcz);
			coordinates.Add(lodz);			
			coordinates.Add(poznan);
			coordinates.Add(mlawa);
			coordinates.Add(szczecin);

			return coordinates.ToArray();
		}

		private static GeoJSON.Net.Geometry.Position[] GetManyCoordinatesForOptimalRoute()
		{
			List<GeoJSON.Net.Geometry.Position> coordinates = new List<GeoJSON.Net.Geometry.Position>();

			GeoJSON.Net.Geometry.Position bydgoszcz = new GeoJSON.Net.Geometry.Position(53.114519, 18.008936);
			GeoJSON.Net.Geometry.Position lodz = new GeoJSON.Net.Geometry.Position(51.785124, 19.462234);
			GeoJSON.Net.Geometry.Position poznan = new GeoJSON.Net.Geometry.Position(52.398515, 16.938702);
			GeoJSON.Net.Geometry.Position mlawa = new GeoJSON.Net.Geometry.Position(53.112128, 20.383661);

			coordinates.Add(bydgoszcz);
			coordinates.Add(lodz);
			coordinates.Add(poznan);
			coordinates.Add(mlawa);

			return coordinates.ToArray();
		}

		private void AddPolylineToMap(ViewModel.Polyline polyline)
		{
			(this.DataContext as MapViewModel).Polylines.Add(polyline);
		}

		//private static System.Windows.Shapes.Polyline GetPolyline()
		//{
		//	string text = File.ReadAllText(@"C:\OSM\route.geojson");
		//	GeoJsonModel parsed = JsonConvert.DeserializeObject<GeoJsonModel>(text);
		//	var polyline = new System.Windows.Shapes.Polyline();
		//	foreach (Feature feature in parsed.features)
		//	{
		//		var startingCoordinates = feature.geometry.coordinates[0] as Newtonsoft.Json.Linq.JArray;
		//		//
		//		var startingPoint = new Point((float)startingCoordinates.Last(), (float)startingCoordinates.First());

		//		var endingCoordinates = feature.geometry.coordinates[1] as Newtonsoft.Json.Linq.JArray;
		//		//
		//		var endingPoint = new Point((float)endingCoordinates.Last(), (float)endingCoordinates.First());

		//		polyline.Points.Add(startingPoint);
		//		polyline.Points.Add(endingPoint);
		//	}

		//	return polyline;
		//}

		private static ViewModel.Polyline GetMapControlPolyLine()
		{
			string text = File.ReadAllText(@"C:\OSM\route.geojson");
			GeoJsonModel parsed = JsonConvert.DeserializeObject<GeoJsonModel>(text);
			var polyline = new ViewModel.Polyline();
			polyline.Locations = new MapControl.LocationCollection();
			foreach (Feature feature in parsed.features)
			{
				if (feature.geometry.coordinates[0] is Newtonsoft.Json.Linq.JArray startingCoordinates)
				{
					polyline.Locations.Add(new MapControl.Location((float)startingCoordinates.Last(), (float)startingCoordinates.First()));
				}

				if (feature.geometry.coordinates[1] is Newtonsoft.Json.Linq.JArray endingCoordinates)
				{
					polyline.Locations.Add(new MapControl.Location((float)endingCoordinates.Last(), (float)endingCoordinates.First()));
				}

				//if (feature.geometry.coordinates[0] is double longitude && feature.geometry.coordinates[1] is double latitude)
				//{
				//	polyline.Locations.Add(new MapControl.Location(latitude, longitude));
				//}
			}

			return polyline;
		}

		private static ViewModel.Polyline GetMapControlPolyLineFromOsrmApi()
		{
			//OsrmJsonRouteModel parsed = APIHelpers.OsrmAPIHelper.GetRouteBetweenPoints(GetManyCoordinatesForNormalRoute());

			GeoJSON.Net.Geometry.Position warszawa = new GeoJSON.Net.Geometry.Position(52.230320, 21.011132);
			GeoJSON.Net.Geometry.Position szczecin = new GeoJSON.Net.Geometry.Position(53.421684, 14.561405);
			OsrmJsonRouteModel parsed = OsrmAPIHelper.GetOptimalRoute(warszawa, szczecin, GetManyCoordinatesForOptimalRoute());

			var polyline = new ViewModel.Polyline();
			polyline.Locations = new MapControl.LocationCollection();
			foreach (var coordinate in parsed.routes[0].geometry.coordinates)
			{
				polyline.Locations.Add(new MapControl.Location((float)coordinate[1], (float)coordinate[0]));

				//if (feature.geometry.coordinates[0] is double longitude && feature.geometry.coordinates[1] is double latitude)
				//{
				//	polyline.Locations.Add(new MapControl.Location(latitude, longitude));
				//}
			}

			return polyline;
		}


		//private static Microsoft.Maps.MapControl.WPF.LocationCollection GetListOfLocations()
		//{
		//	string text = File.ReadAllText(@"C:\GIT\private\Mapsui_mytest\Itinero_test\bin\x64\Debug\route.geojson");
		//	GeoJsonModel parsed = JsonConvert.DeserializeObject<GeoJsonModel>(text);
		//	var locs = new Microsoft.Maps.MapControl.WPF.LocationCollection();

		//	foreach (Feature feature in parsed.features)
		//	{
		//		if (feature.geometry.coordinates[0] is Newtonsoft.Json.Linq.JArray startingCoordinates)
		//		{
		//			//
		//			locs.Add(new Microsoft.Maps.MapControl.WPF.Location((float)startingCoordinates.Last(), (float)startingCoordinates.First()));
		//		}

		//		///var startingPoint = new Point((double)startingCoordinates.Last(), (double)startingCoordinates.First());

		//		if (feature.geometry.coordinates[1] is Newtonsoft.Json.Linq.JArray endingCoordinates)
		//		{
		//			locs.Add(new Microsoft.Maps.MapControl.WPF.Location((float)endingCoordinates.Last(), (float)endingCoordinates.First()));
		//		}

		//		if (feature.geometry.coordinates[0] is double longitude && feature.geometry.coordinates[1] is double latitude)
		//		{
		//			locs.Add(new Microsoft.Maps.MapControl.WPF.Location(latitude, longitude));
		//		}
		//		//
		//		///var endingPoint = new Point((float)endingCoordinates.Last(), (float)endingCoordinates.First());

		//		//polyline.Points.Add(startingPoint);
		//		//polyline.Points.Add(endingPoint);
		//	}

		//	return locs;
		//}

		private static Itinero.Route GetRoute()
		{
			RouterDb routerDb = null;
			using (FileStream stream = new FileInfo(@"C:\GIT\private\map\quebec.routerdb").OpenRead())
			{
				routerDb = RouterDb.Deserialize(stream);
			}
			var router = new Itinero.Router(routerDb);

			//get a profile
			Itinero.Profiles.Profile profile = Vehicle.Car.Fastest(); //the default OSM car profile


			//routerDb.AddContracted(profile); //dodawanie tego trwa bardzo długo, może się opłacać zrobić to przed wyznaczaniem wielu tras

			var from = new Itinero.LocalGeo.Coordinate(45.532400f, -73.622885f);
			var to = new Itinero.LocalGeo.Coordinate(45.545841f, -73.623474f);

			//create a routerpoint from a location
			//snaps the given location to the nearest routable edge
			RouterPoint start = router.TryResolve(profile, from, 200).Value;
			RouterPoint end = router.TryResolve(profile, to, 200).Value;

			var points = new List<RouterPoint>()
			{
				router.TryResolve(profile, from, 200).Value,
				router.TryResolve(profile, to, 200).Value
			};



			//calculate a route
			//var route = router.Calculate(profile, start, end);
			Result<Itinero.Route> route = router.TryCalculate(profile, points.ToArray());
			return route.Value;
		}




		private void MapMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				//map.ZoomMap(e.GetPosition(map), Math.Floor(map.ZoomLevel + 1.5));
				//map.ZoomToBounds(new BoundingBox(53, 7, 54, 9));
				this.map.TargetCenter = this.map.ViewToLocation(e.GetPosition(this.map));
			}
		}

		private void MapMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				//map.ZoomMap(e.GetPosition(map), Math.Ceiling(map.ZoomLevel - 1.5));
			}
		}

		private void MapMouseMove(object sender, MouseEventArgs e)
		{
			MapControl.Location location = this.map.ViewToLocation(e.GetPosition(this.map));
			int latitude = (int)Math.Round(location.Latitude * 60000d);
			int longitude = (int)Math.Round(MapControl.Location.NormalizeLongitude(location.Longitude) * 60000d);
			char latHemisphere = 'N';
			char lonHemisphere = 'E';

			if (latitude < 0)
			{
				latitude = -latitude;
				latHemisphere = 'S';
			}

			if (longitude < 0)
			{
				longitude = -longitude;
				lonHemisphere = 'W';
			}

			this.mouseLocation.Text = string.Format(CultureInfo.InvariantCulture,
				"{0}  {1:00} {2:00.000}\n{3} {4:000} {5:00.000}",
				latHemisphere, latitude / 60000, (latitude % 60000) / 1000d,
				lonHemisphere, longitude / 60000, (longitude % 60000) / 1000d);
		}

		private void MapMouseLeave(object sender, MouseEventArgs e)
		{
			this.mouseLocation.Text = string.Empty;
		}

		private void MapManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
		{
			e.TranslationBehavior.DesiredDeceleration = 0.001;
		}

		private void MapItemTouchDown(object sender, TouchEventArgs e)
		{
			var mapItem = (MapItem)sender;
			mapItem.IsSelected = !mapItem.IsSelected;
			e.Handled = true;
		}

		private void SeamarksChecked(object sender, RoutedEventArgs e)
		{
			this.map.Children.Insert(this.map.Children.IndexOf(this.mapGraticule), ((MapViewModel)this.DataContext).MapLayers.SeamarksLayer);
		}

		private void SeamarksUnchecked(object sender, RoutedEventArgs e)
		{
			this.map.Children.Remove(((MapViewModel)this.DataContext).MapLayers.SeamarksLayer);
		}
	}
}

using Itinero;
using MapControl;
using NetTopologySuite.IO;
using OsmSharp.API;
using Router;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PointItem : ViewModelBase
    {
        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.RaisePropertyChanged(nameof(this.Name));
            }
        }

        private Location location;
        public Location Location
        {
            get { return this.location; }
            set
            {
                this.location = value;
                this.RaisePropertyChanged(nameof(this.Location));
            }
        }
    }

    public class Polyline
    {
		public Polyline()
		{
			this.Color = "Brown";
			this.StrokeThickness = 3;
		}


        public LocationCollection Locations { get; set; }

        public string Color { get; set; }

        public int StrokeThickness { get; set; }
    }

    public class MapViewModel : ViewModelBase
    {
        private void UserInputData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CanStartRouteCalculation = this.CanCalculateRoute(sender);                 
        }

        private Location mapCenter = new Location(52.237049f, 21.017532f);
        public Location MapCenter
        {
            get { return this.mapCenter; }
            set
            {
                this.mapCenter = value;
                this.RaisePropertyChanged(nameof(this.MapCenter));
            }
        }

        public ObservableCollection<PointItem> Points { get; } = new ObservableCollection<PointItem>();
        public ObservableCollection<PointItem> Pushpins { get; } = new ObservableCollection<PointItem>();
        public ObservableCollection<Polyline> Polylines { get; } = new ObservableCollection<Polyline>();

        public MapLayers MapLayers { get; } = new MapLayers();

        public UserInputData UserInputData { get; }

        public ICommand CalculateRoute
        {
            get
            {
                return new RelayCommand(this.CalculateRouteExecute, this.CanCalculateRoute);
            }
        }
        private void CalculateRouteExecute()
        {
            this.UserInputData.ProcessingTime = "During processing";
            var watch = new Stopwatch();
            watch.Start();
            this.Polylines.Clear();
            var startingPosition = Router.APIHelpers.NominatimAPIHelper.GetPositionForAddress(this.UserInputData.StartingPoint);
            var endingPosition = Router.APIHelpers.NominatimAPIHelper.GetPositionForAddress(this.UserInputData.EndingPoint);
            GeoJSON.Net.Geometry.Position startingPoint = new GeoJSON.Net.Geometry.Position(startingPosition.Latitude, startingPosition.Longitude);
            GeoJSON.Net.Geometry.Position endingPoint = new GeoJSON.Net.Geometry.Position(endingPosition.Latitude, endingPosition.Longitude);
            
            double additionalTime = double.Parse(this.UserInputData.AdditionalTimeMin) * 60;
            double additionalDistance = double.Parse(this.UserInputData.AdditionalDistanceKm) * 1000;

            var router = new Router.Router(startingPoint, endingPoint, additionalDistance, additionalTime);
            Router.Model.RouteModel route = router.GetRoute(true);
            Polyline polyline = this.GetFromMultiPoint(route.MultiPoint);
            polyline.Color = "Blue";
            polyline.StrokeThickness = 6;
            this.Polylines.Add(polyline);

            Router.Model.RouteModel referenceRoute = router.ReferenceRoute;
            Polyline polylineReference = this.GetFromMultiPoint(referenceRoute.MultiPoint);
            polylineReference.Color = "Red"; 
            polylineReference.StrokeThickness = 3; 
            this.Polylines.Add(polylineReference);

            List<PointItem> pointItemsRoute = this.GetFromRouteModel(route);
            foreach(var pointItem in pointItemsRoute)
            {
                this.Pushpins.Add(pointItem);
            }
            watch.Stop();
            this.UserInputData.ProcessingTime = $"{watch.Elapsed.Minutes} min {watch.Elapsed.Seconds} sec";
        }
        private bool CanCalculateRoute(object obj)
        {
            return !string.IsNullOrWhiteSpace(this.UserInputData.AdditionalDistanceKm)
                && double.TryParse(this.UserInputData.AdditionalDistanceKm, out _)
                && !string.IsNullOrWhiteSpace(this.UserInputData.AdditionalTimeMin)
                && double.TryParse(this.UserInputData.AdditionalTimeMin, out _)
                && !string.IsNullOrWhiteSpace(this.UserInputData.StartingPoint)
                && !string.IsNullOrWhiteSpace(this.UserInputData.EndingPoint);
        }

        private Polyline GetFromMultiPoint(NetTopologySuite.Geometries.MultiPoint multiPoint)
        {
            var result = new Polyline()
            {
                Locations = new LocationCollection()
            };

            foreach (NetTopologySuite.Geometries.Coordinate point in multiPoint.Coordinates)
            {
                result.Locations.Add(new Location(point.Y, point.X));
            }
            return result;
        }

        private List<PointItem> GetFromRouteModel(Router.Model.RouteModel routeModel)
        {
            var result = new List<PointItem>();

            for(int i = 0; i < routeModel.Waypoints.Length; i++) 
            {
                int waypointIndex = routeModel.Waypoints[i].waypoint_index;

                PointItem pointItem = new PointItem()
                {
                    Name = $"{++waypointIndex}. Nazwa: {routeModel.Waypoints[i].name}",
                    Location = new Location(routeModel.Waypoints[i].location[1], routeModel.Waypoints[i].location[0])
                };
                result.Add(pointItem);
            }

            return result;
        }
        
        private bool canStartRouteCalculation;
        public bool CanStartRouteCalculation
        {
            get => this.canStartRouteCalculation;
            set
            {
                this.canStartRouteCalculation = value;
                this.RaisePropertyChanged(nameof(this.CanStartRouteCalculation));
            }
        }

        public MapViewModel()
        {
            this.UserInputData = new UserInputData();
            this.UserInputData.PropertyChanged += this.UserInputData_PropertyChanged;
        }
    }
}

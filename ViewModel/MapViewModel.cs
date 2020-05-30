using DbModel;
using Itinero;
using MapControl;
using NetTopologySuite.IO;
using OsmSharp.API;
using Router;
using System;
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

        public string BackgroundColor { get; set; }
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

        public ObservableCollection<PointItem> PushpinsRoute { get; } = new ObservableCollection<PointItem>();

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
            // TODO: this one line below does not seem to work
            this.UserInputData.ProcessingTime = "During processing";
            var watch = new Stopwatch();
            watch.Start();
            this.Polylines.Clear();
            var startingPosition = Router.APIHelpers.NominatimAPIHelper.GetPositionForAddress(this.UserInputData.StartingPoint);
            var endingPosition = Router.APIHelpers.NominatimAPIHelper.GetPositionForAddress(this.UserInputData.EndingPoint);
            
            double additionalTime = double.Parse(this.UserInputData.AdditionalTimeMin) * 60;
            double additionalDistance = double.Parse(this.UserInputData.AdditionalDistanceKm) * 1000;

            var router = new Router.Router(startingPosition, endingPosition, additionalDistance, additionalTime);
            Router.Model.RouteModel route = router.GetRoute(this.UserInputData.UseAggregatedPoints);
            Polyline polyline = this.GetFromMultiPoint(route.MultiPoint, "Blue", 6);
            this.Polylines.Add(polyline);
            this.UserInputData.ResultDistanceKm = (route.Distance / 1000).ToString();
            this.UserInputData.ResultTimeHMin = GetHoursMinutesFromSeconds(route.Time);
            this.UserInputData.ResultAdditionalStops = (route.Waypoints.Length - 2).ToString();

            Router.Model.RouteModel referenceRoute = router.ReferenceRoute;
            Polyline polylineReference = this.GetFromMultiPoint(referenceRoute.MultiPoint, "Red", 3);
            this.Polylines.Add(polylineReference);
            this.UserInputData.ReferenceDistanceKm = (referenceRoute.Distance / 1000).ToString();
            this.UserInputData.ReferenceTimeHMin = GetHoursMinutesFromSeconds(referenceRoute.Time);

            List<PointItem> pointItemsRoute = this.GetFromRouteModel(route);
            this.PushpinsRoute.Clear();
            foreach (var pointItem in pointItemsRoute)
            {                
                this.PushpinsRoute.Add(pointItem);
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

        private string GetHoursMinutesFromSeconds(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);

            string answer = string.Format("{0:D2}h {1:D2}min",
                            t.Hours,
                            t.Minutes);
            return answer;
        }


        private Polyline GetFromMultiPoint(NetTopologySuite.Geometries.MultiPoint multiPoint, string polylineColor, int polylineStrokeThickness)
        {
            var result = new Polyline()
            {
                Locations = new LocationCollection()
            };

            foreach (NetTopologySuite.Geometries.Coordinate point in multiPoint.Coordinates)
            {
                result.Locations.Add(new Location(point.Y, point.X));
            }

            result.Color = polylineColor;
            result.StrokeThickness = polylineStrokeThickness;
            return result;
        }

        private List<PointItem> GetFromRouteModel(Router.Model.RouteModel routeModel)
        {
            var result = new List<PointItem>();

            PointItem startItem = new PointItem()
            {
                Name = $"1. {this.UserInputData.StartingPoint}",
                Location = new Location(routeModel.Waypoints[0].location[1], routeModel.Waypoints[0].location[0]),
                BackgroundColor = "LightGreen"
            };
            PointItem endItem = new PointItem()
            {
                Name = $"{routeModel.Waypoints.Length}. {this.UserInputData.EndingPoint}",
                Location = new Location(routeModel.Waypoints[routeModel.Waypoints.Length - 1].location[1], routeModel.Waypoints[routeModel.Waypoints.Length - 1].location[0]),
                BackgroundColor = "LightGreen"
            };
            result.Add(startItem);
            result.Add(endItem);

            for (int i = 1; i < routeModel.Waypoints.Length - 1; i++) 
            {
                int waypointIndex = routeModel.Waypoints[i].waypoint_index;

                PointItem pointItem = new PointItem()
                {
                    Name = $"{++waypointIndex}.",
                    Location = new Location(routeModel.Waypoints[i].location[1], routeModel.Waypoints[i].location[0]),
                    BackgroundColor = "LightGreen"
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

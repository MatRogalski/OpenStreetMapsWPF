using Itinero;
using MapControl;
using NetTopologySuite.IO;
using OsmSharp.API;
using Router;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public LocationCollection Locations { get; set; }
    }

    public class MapViewModel : ViewModelBase
    {
        private void UserInputData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.CanStartRouteCalculation = this.CanCalculateRoute(sender);                 
        }

        private Location mapCenter = new Location(53.5, 8.2);
        //private Location mapCenter = new Location(45.532400f, -73.622885f);
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
            var startingPosition = Router.APIHelpers.NominatimAPIHelper.GetPositionForAddress(this.UserInputData.StartingPoint);
            var endingPosition = Router.APIHelpers.NominatimAPIHelper.GetPositionForAddress(this.UserInputData.EndingPoint);

            NetTopologySuite.Geometries.Point startingPoint = new NetTopologySuite.Geometries.Point(startingPosition.Longitude, startingPosition.Latitude);
            NetTopologySuite.Geometries.Point endingPoint = new NetTopologySuite.Geometries.Point(endingPosition.Longitude, endingPosition.Latitude);


            double additionalTime = double.Parse(this.UserInputData.AdditionalTimeMin);
            double additionalDistance = double.Parse(this.UserInputData.AdditionalDistanceKm) * 1000;
            var router = new Router.Router(startingPoint, endingPoint, additionalDistance, additionalTime);
            Router.Model.RouteModel route = router.GetRoute(false);
            Polyline polyline = this.GetFromMultiPoint(route.MultiPoint);
            this.Polylines.Add(polyline);
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

            this.Points.Add(new PointItem
            {
                Name = "Steinbake Leitdamm",
                Location = new Location(53.51217, 8.16603)
            });
            this.Points.Add(new PointItem
            {
                Name = "Buhne 2",
                Location = new Location(53.50926, 8.15815)
            });
            this.Points.Add(new PointItem
            {
                Name = "Buhne 4",
                Location = new Location(53.50468, 8.15343)
            });
            this.Points.Add(new PointItem
            {
                Name = "Buhne 6",
                Location = new Location(53.50092, 8.15267)
            });
            this.Points.Add(new PointItem
            {
                Name = "Buhne 8",
                Location = new Location(53.49871, 8.15321)
            });
            this.Points.Add(new PointItem
            {
                Name = "Buhne 10",
                Location = new Location(53.49350, 8.15563)
            });

            this.Pushpins.Add(new PointItem
            {
                Name = "WHV - Eckwarderhörne",
                Location = new Location(53.5495, 8.1877)
            });
            this.Pushpins.Add(new PointItem
            {
                Name = "JadeWeserPort",
                Location = new Location(53.5914, 8.14)
            });
            this.Pushpins.Add(new PointItem
            {
                Name = "Kurhaus Dangast",
                Location = new Location(53.447, 8.1114)
            });
            this.Pushpins.Add(new PointItem
            {
                Name = "Eckwarderhörne",
                Location = new Location(53.5207, 8.2323)
            });

            this.Polylines.Add(new Polyline
            {
                Locations = LocationCollection.Parse("53.5140,8.1451 53.5123,8.1506 53.5156,8.1623 53.5276,8.1757 53.5491,8.1852 53.5495,8.1877 53.5426,8.1993 53.5184,8.2219 53.5182,8.2386 53.5195,8.2387")
            });
            this.Polylines.Add(new Polyline
            {
                Locations = LocationCollection.Parse("53.5978,8.1212 53.6018,8.1494 53.5859,8.1554 53.5852,8.1531 53.5841,8.1539 53.5802,8.1392 53.5826,8.1309 53.5867,8.1317 53.5978,8.1212")
            });
        }
    }
}

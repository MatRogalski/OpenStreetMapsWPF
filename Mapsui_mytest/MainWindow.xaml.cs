using Mapsui;
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
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Projection;
using Itinero;
using System.IO;
using Itinero.IO.Osm;
using Itinero.Osm.Vehicles;

namespace Mapsui_mytest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));

			var eitiCoordinates = new Point(21.011769094336856, 52.2190040831747);

			var sphericalMarcatorCoordinate = SphericalMercator.FromLonLat(eitiCoordinates.X, eitiCoordinates.Y);

			MyMapControl.Map.Home = n => n.NavigateTo(sphericalMarcatorCoordinate, MyMapControl.Map.Resolutions[17]);



		}
	}
}

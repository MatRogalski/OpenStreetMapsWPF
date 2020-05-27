namespace Router.Model
{

#pragma warning disable IDE1006 // Naming Styles
	public class GeoJsonModel
	{
		public string type { get; set; }
		public Feature[] features { get; set; }
	}

	public class Feature
	{
		public string type { get; set; }
		public string name { get; set; }
		public GeometryGeoJson geometry { get; set; }
		public Properties properties { get; set; }
		public string Shape { get; set; }
	}

	public class GeometryGeoJson
	{
		public string type { get; set; }
		public object[] coordinates { get; set; }
	}

	public class Properties
	{
		public string name { get; set; }
		public string highway { get; set; }
		public string profile { get; set; }
		public string distance { get; set; }
		public string time { get; set; }
		public string oneway { get; set; }
	}
#pragma warning restore IDE1006 // Naming Styles
}
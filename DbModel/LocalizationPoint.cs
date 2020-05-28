using GeoJSON.Net.Geometry;
using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DbModel
{
	public class LocalizationPoint
	{
		[Key]
		public long? PointId { get; set; }
		public Point Point { get; set; }
		public string PostalCode { get; set; }
		public string Region { get; set; }
		public string District { get; set; }
		public string City { get; set; }
		public string Street { get; set; }
		public string Number { get; set; }


		public double? StaticScore { get; set; }
		public double? InnerDistance { get; set; }
		public double? InnerTime { get; set; }

		[NotMapped]
		public double? DynamicScore { get; set; }


		public long? ParentPointId { get; set; }
		public LocalizationPoint ParentPoint { get; set; }

		public virtual List<LocalizationPoint> RelatedPoints { get; set; }

	}
}

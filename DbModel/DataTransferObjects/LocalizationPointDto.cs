using GeoJSON.Net.Geometry;
using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DbModel
{
	public class LocalizationPointDto
	{
		public LocalizationPointDto()
		{

		}

		public LocalizationPointDto(LocalizationPoint localizationPoint)
		{
			this.PointId = localizationPoint.PointId;
			//X is longitude, Y is latitude in NetTopologySuite.Geometries.Coordinate Class
			this.Point = new Point(new Position(localizationPoint.Point.Coordinate.Y, localizationPoint.Point.Coordinate.X));
			this.PostalCode = localizationPoint.PostalCode;
			this.Region = localizationPoint.Region;
			this.District = localizationPoint.District;
			this.City = localizationPoint.City;
			this.Street = localizationPoint.Street;
			this.Number = localizationPoint.Number;
			this.StaticScore = localizationPoint.StaticScore;
			this.InnerDistance = localizationPoint.InnerDistance;
			this.InnerTime = localizationPoint.InnerTime;
			this.DynamicScore = localizationPoint.DynamicScore;
			this.ParentPointId = localizationPoint.ParentPointId;
		}

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

		public double? DynamicScore { get; set; }


		public long? ParentPointId { get; set; }

		//method to use in repo
		public void GetDataFromLocalizationPoint (LocalizationPoint localizationPoint)
		{
			//X is longitude, Y is latitude in NetTopologySuite.Geometries.Coordinate Class
			this.Point = new Point(new Position(localizationPoint.Point.Coordinate.Y, localizationPoint.Point.Coordinate.X));
			this.PostalCode = localizationPoint.PostalCode;
			this.Region = localizationPoint.Region;
			this.District = localizationPoint.District;
			this.City = localizationPoint.City;
			this.Street = localizationPoint.Street;
			this.Number = localizationPoint.Number;
			this.StaticScore = localizationPoint.StaticScore;
			this.InnerDistance = localizationPoint.InnerDistance;
			this.InnerTime = localizationPoint.InnerTime;
			this.DynamicScore = localizationPoint.DynamicScore;
			this.ParentPointId = localizationPoint.ParentPointId;
		}

		//method to use in repo
		public void GetDataFromLocalizationPointDto (LocalizationPointDto localizationPointDto)
		{
			this.Point = localizationPointDto.Point;
			this.PostalCode = localizationPointDto.PostalCode;
			this.Region = localizationPointDto.Region;
			this.District = localizationPointDto.District;
			this.City = localizationPointDto.City;
			this.Street = localizationPointDto.Street;
			this.Number = localizationPointDto.Number;
			this.StaticScore = localizationPointDto.StaticScore;
			this.InnerDistance = localizationPointDto.InnerDistance;
			this.InnerTime = localizationPointDto.InnerTime;
			this.DynamicScore = localizationPointDto.DynamicScore;
			this.ParentPointId = localizationPointDto.ParentPointId;
		}

	}
}

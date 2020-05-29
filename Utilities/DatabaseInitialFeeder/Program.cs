using DbConnector.Repositories;
using DbModel;
using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DatabaseInitialFeeder
{
	class Program
	{
		static void Main(string[] args)
		{
			InsertDataFromFileToDb(@$"C:\GIT\private\map\mazowieckie_100.csv");

		}

		private static void InsertDataFromFileToDb(string fileName)
		{
			string[] lines = File.ReadAllLines(fileName);
			var repo = new LocalizationPointRepository();

			foreach (string line in lines)
			{
				string[] splitted = line.Split(",", StringSplitOptions.None);

				if (splitted.Count() >= 9
					&& double.TryParse(splitted[0], out double longitude)
					&& double.TryParse(splitted[1], out double latitude))
				{


					var localizationPoint = new LocalizationPointDto()
					{
						Point = new Point(new Position(latitude,longitude)),
						Number = splitted[2],
						Street = splitted[3],
						City = splitted[5],
						District = splitted[6],
						Region = splitted[7],
						PostalCode = splitted[8],
						StaticScore = 0,
						InnerDistance = 0,
						InnerTime = 0
					};

					repo.Add(localizationPoint);
				}
			}
			repo.SaveChanges();
		}
	}
}

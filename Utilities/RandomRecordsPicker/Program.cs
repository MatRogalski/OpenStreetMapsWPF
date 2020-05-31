using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RandomRecordsPicker
{
	class Program
	{
		static void Main(string[] args)
		{
			int numberOfResults = 15;
			GetNRecordsFromFile(numberOfResults, @"C:\OSM\mazowieckie_100.csv", @$"C:\OSM\mazowieckie_{numberOfResults}.csv");

			//GetNRecordsFromDirectory(numberOfResults, @"C:\GIT\private\map\openaddr-collected-europe\pl\", @$"C:\GIT\private\map\polska_{numberOfResults}.csv");
		}

		private static void GetNRecordsFromFile(int numberOfResults, string sourcePath, string destinationPath)
		{
			var lines = File.ReadAllLines(sourcePath).ToList();

			lines.RemoveAt(0);

			var toSave = lines.OrderBy(x => Guid.NewGuid()).Take(numberOfResults).ToList();

			string directory = Path.GetDirectoryName(destinationPath);
			if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using (var writer = new StreamWriter(destinationPath))
			{
				foreach (string line in toSave)
				{
					writer.WriteLine(line);
				}
			}
		}
		
		private static void GetNRecordsFromDirectory(int numberOfResults, string sourceDirectoryPath, string destinationPath)
		{
			var filesInDirectory = Directory.GetFiles(sourceDirectoryPath).Where(i => i.Contains(".csv")).ToList();

			var lines = new List<string>();

			foreach (string file in filesInDirectory)
			{
				var linesFromFile = File.ReadAllLines(file).ToList();
				linesFromFile.RemoveAt(0);
				
				lines.AddRange(linesFromFile);
			}


			var toSave = lines.OrderBy(x => Guid.NewGuid()).Take(numberOfResults).ToList();

			string directory = Path.GetDirectoryName(destinationPath);
			if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			using (var writer = new StreamWriter(destinationPath))
			{
				foreach (string line in toSave)
				{
					writer.WriteLine(line);
				}
			}
		}
	}
}

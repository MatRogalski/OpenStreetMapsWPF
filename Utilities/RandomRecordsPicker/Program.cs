using System;
using System.IO;
using System.Linq;

namespace RandomRecordsPicker
{
	class Program
	{
		static void Main(string[] args)
		{
			int numberOfResults = 100;
			GetNRecordsFromFile(numberOfResults, @"C:\GIT\private\map\openaddr-collected-europe\pl\mazowieckie.csv", @$"C:\GIT\private\map\mazowieckie_{numberOfResults}.csv");
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
	}
}

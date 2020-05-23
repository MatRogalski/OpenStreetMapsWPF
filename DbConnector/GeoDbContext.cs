using DbModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;

namespace DbConnector
{

	//In package manager console
	//Add-Migration -Project DbConnector -Name InitialMigration
	//Update-Database -Project DbConnector
	//stare: Update-Database -ProjectName DbConnector -Force -Verbose -SourceMigration InitialMigration -StartUpProjectName DbConnector
	//[notmapped]
	public class GeoDbContext : DbContext
	{
		public DbSet<LocalizationPoint> LocalizationPoints { get; set; }


		public GeoDbContext() : base()
		{

		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json")
				.Build();

			optionsBuilder.UseSqlServer(configuration.GetConnectionString("GeoDb"), x=>x.UseNetTopologySuite());
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<LocalizationPoint>()
				.HasMany(i => i.RelatedPoints)
				.WithOne(p => p.ParentPoint)
				.HasForeignKey(p => p.ParentPointId)
				.OnDelete(DeleteBehavior.NoAction);

		}
	}
}

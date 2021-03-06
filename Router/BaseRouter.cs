﻿using DbConnector.Repositories;
using DbModel;
using GeoJSON.Net.Geometry;
using Router.APIHelpers;
using Router.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Router
{
    public abstract class BaseRouter : IRouter
    {
		protected readonly Position startingPosition;
		protected readonly Position endingPositions;
		protected readonly double totalAdditionalDistance;
		protected readonly double totalAdditionalTime;
		protected double maxAllowedRouteDistance;
		protected double maxAllowedRouteTime;
		protected readonly ILocalizationPointRepository repo;
		protected readonly List<Position> waypoints;
		protected RouteModel referenceRoute;
		protected RouteModel resultRoute;
		protected bool doesRouteMeetParameters;

		public RouteModel ReferenceRoute { get { return referenceRoute; } }
		public List<LocalizationPointDto> PointsFromRepo { get; private set; }

		protected BaseRouter()
		{
			this.repo = new LocalizationPointRepository();
			//this.repo = new MockLocalizationPointRepositoryMazowieckie();
			this.waypoints = new List<Position>();
		}

		public BaseRouter(Position startingPoint, Position endingPoint, double additionalDistance, double additionalTime) : this()
		{
			this.startingPosition = startingPoint;
			this.endingPositions = endingPoint;
			this.totalAdditionalDistance = additionalDistance;
			this.totalAdditionalTime = additionalTime;
		}

		public RouteModel GetRoute(bool useAggregatedPoints)
		{
			this.referenceRoute = this.GetRouteBetweenTwoPoints();
			this.maxAllowedRouteDistance = this.referenceRoute.Distance + this.totalAdditionalDistance;
			this.maxAllowedRouteTime = this.referenceRoute.Time + this.totalAdditionalTime;

			this.resultRoute = this.referenceRoute;
			this.PointsFromRepo = (useAggregatedPoints ? this.repo.GetWithAggregated() : this.repo.GetWithoutAggregated()).ToList();
			var availablePoints = this.PointsFromRepo;
			this.ProcessAvailablePoints(availablePoints, this.totalAdditionalDistance, this.totalAdditionalTime);
			return this.resultRoute;
		}

		protected abstract void ProcessAvailablePoints(List<LocalizationPointDto> availablePoints, double currentAdditionalDistance, double currentAdditionalTime, int? stepSize = null);

		protected bool DoesRouteMeetParameters(RouteModel route, double additionalDistance, double additionalTime)
		{
			if (route.Distance > this.maxAllowedRouteDistance)
				return false;
			if (route.Time > this.maxAllowedRouteTime)
				return false;

			return true;
		}

		protected RouteModel GetRouteBetweenTwoPoints()
		{
			//var routeJson = MapboxAPIHelper.GetSimpleRoute(startingPosition, endingPositions);
			var routeJson = OsrmAPIHelper.GetSimpleRoute(startingPosition, endingPositions);
			return routeJson.ToRouteModel();
		}


		protected RouteModel GetRouteBetweenTwoPoints(List<Position> waypoints)
		{
			//var routeJson = MapboxAPIHelper.GetOptimalRoute(this.startingPosition, this.endingPositions, waypoints.ToArray());
			var routeJson = OsrmAPIHelper.GetOptimalRoute(this.startingPosition, this.endingPositions, waypoints.ToArray());
			return routeJson.ToRouteModel();
		}

	}
}

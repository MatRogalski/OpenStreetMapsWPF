using Router.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Router
{
    public interface IRouter
    {
        RouteModel GetRoute(bool useAggregatedPoints);
        public RouteModel ReferenceRoute { get; }
    }
}

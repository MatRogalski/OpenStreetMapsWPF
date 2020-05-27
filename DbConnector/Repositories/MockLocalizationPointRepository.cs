using DbModel;
using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Repositories
{
    public class MockLocalizationPointRepository : ILocalizationPointRepository
    {
        private List<LocalizationPoint> points = new List<LocalizationPoint>();

        public MockLocalizationPointRepository()
        {
        //pajeczno: 51.144550, 18.999704
        //     sulmierzyce = 51.185689, 19.194213
        //     marchewki 51.186707, 18.937751
        //     wielun 51.220383, 18.564313
        //     lask 51.589554, 19.136186
        //     kamien 51.261527, 19.203991
            LocalizationPoint pajeczno = new LocalizationPoint()
            {
                PointId = 1,
                Point = new Point(new Position(51.144550, 18.999704)),
                StaticScore = 0
            };
            LocalizationPoint sulmierzyce = new LocalizationPoint()
            {
                PointId = 1,
                Point = new Point(new Position(51.185689, 19.194213)),
                StaticScore = 0
            };
            LocalizationPoint marchewki = new LocalizationPoint()
            {
                PointId = 1,
                Point = new Point(new Position(51.186707, 18.937751)),
                StaticScore = 0
            };
            LocalizationPoint wielun = new LocalizationPoint()
            {
                PointId = 1,
                Point = new Point(new Position(51.220383, 18.564313)),
                StaticScore = 0
            };
            LocalizationPoint lask = new LocalizationPoint()
            {
                PointId = 1,
                Point = new Point(new Position(51.589554, 19.136186)),
                StaticScore = 0
            };
            LocalizationPoint kamien = new LocalizationPoint()
            {
                PointId = 1,
                Point = new Point(new Position(51.261527, 19.203991)),
                StaticScore = 0
            };

            points.Add(pajeczno);
            points.Add(sulmierzyce);
            points.Add(marchewki);
            points.Add(wielun);
            points.Add(lask);
            points.Add(kamien);
        }

        public List<LocalizationPoint> GetByParentId(long parentLocalizationPointId)
        {
            throw new NotImplementedException();
        }

        public List<LocalizationPoint> GetWithAggregated()
        {
            throw new NotImplementedException();
        }

        public List<LocalizationPoint> GetWithoutAggregated()
        {
            return points;
        }
    }
}

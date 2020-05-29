using DbModel;
using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Repositories
{
    public class MockLocalizationPointRepository : ILocalizationPointRepository
    {
        private List<LocalizationPointDto> points = new List<LocalizationPointDto>();

        public MockLocalizationPointRepository()
        {
        //pajeczno: 51.144550, 18.999704
        //     sulmierzyce = 51.185689, 19.194213
        //     marchewki 51.186707, 18.937751
        //     wielun 51.220383, 18.564313
        //     lask 51.589554, 19.136186
        //     kamien 51.261527, 19.203991
            LocalizationPointDto pajeczno = new LocalizationPointDto()
            {
                PointId = 1,
                Point = new Point(new Position(51.144550, 18.999704)),
                StaticScore = 0
            };
            LocalizationPointDto sulmierzyce = new LocalizationPointDto()
            {
                PointId = 1,
                Point = new Point(new Position(51.185689, 19.194213)),
                StaticScore = 0
            };
            LocalizationPointDto marchewki = new LocalizationPointDto()
            {
                PointId = 1,
                Point = new Point(new Position(51.186707, 18.937751)),
                StaticScore = 0
            };
            LocalizationPointDto wielun = new LocalizationPointDto()
            {
                PointId = 1,
                Point = new Point(new Position(51.220383, 18.564313)),
                StaticScore = 0
            };
            LocalizationPointDto lask = new LocalizationPointDto()
            {
                PointId = 1,
                Point = new Point(new Position(51.589554, 19.136186)),
                StaticScore = 0
            };
            LocalizationPointDto kamien = new LocalizationPointDto()
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

        public List<LocalizationPointDto> GetByParentId(long parentLocalizationPointDtoId)
        {
            throw new NotImplementedException();
        }

        public List<LocalizationPointDto> GetWithAggregated()
        {
            throw new NotImplementedException();
        }

        public List<LocalizationPointDto> GetWithoutAggregated()
        {
            return points;
        }
    }
}

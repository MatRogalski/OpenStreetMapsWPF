using DbModel;
using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Repositories
{
    public class MockLocalizationPointRepositoryMazowieckie : ILocalizationPointRepository
    {
        private List<LocalizationPointDto> points = new List<LocalizationPointDto>();

        public MockLocalizationPointRepositoryMazowieckie()
        {
            //plock: 52.546796, 19.708701
            //ciolkowo: 52.604040, 19.879803
            //sierpc: 52.855707, 19.668891
            //torun: 53.013152, 18.611620
            //ciechanow: 52.868006, 20.625774
            //przasnysz: 53.016507, 20.884518
            //lomza: 53.168437, 22.064546
            //dzialdowo: 53.231384, 20.167872
            //naruszewo: 52.525421, 20.351985
            //rzewin: 52.724651, 20.277417

            LocalizationPointDto plock = new LocalizationPointDto()
            {
                PointId = 1,
                Point = new Point(new Position(52.546796, 19.708701)),
                StaticScore = 0
            };
            LocalizationPointDto ciolkowo = new LocalizationPointDto()
            {
                PointId = 2,
                Point = new Point(new Position(52.604040, 19.879803)),
                StaticScore = 0
            };
            LocalizationPointDto sierpc = new LocalizationPointDto()
            {
                PointId = 3,
                Point = new Point(new Position(52.855707, 19.668891)),
                StaticScore = 0
            };
            LocalizationPointDto torun = new LocalizationPointDto()
            {
                PointId = 4,
                Point = new Point(new Position(53.013152, 18.611620)),
                StaticScore = 0
            };
            LocalizationPointDto ciechanow = new LocalizationPointDto()
            {
                PointId = 5,
                Point = new Point(new Position(52.868006, 20.625774)),
                StaticScore = 0
            };
            LocalizationPointDto przasnysz = new LocalizationPointDto()
            {
                PointId = 6,
                Point = new Point(new Position(53.016507, 20.884518)),
                StaticScore = 0
            };

            LocalizationPointDto lomza = new LocalizationPointDto()
            {
                PointId = 7,
                Point = new Point(new Position(53.168437, 22.064546)),
                StaticScore = 0
            };
            LocalizationPointDto dzialdowo = new LocalizationPointDto()
            {
                PointId = 8,
                Point = new Point(new Position(53.229588, 20.167984)),
                StaticScore = 0
            };
            LocalizationPointDto naruszewo = new LocalizationPointDto()
            {
                PointId = 9,
                Point = new Point(new Position(52.525421, 20.351985)),
                StaticScore = 0
            };
            LocalizationPointDto rzewin = new LocalizationPointDto()
            {
                PointId = 10,
                Point = new Point(new Position(52.724651, 20.277417)),
                StaticScore = 0
            };
            LocalizationPointDto plock2 = new LocalizationPointDto()
            {
                PointId = 11,
                Point = new Point(new Position(52.542984, 19.687158)),
                StaticScore = 0
            };
            LocalizationPointDto plock3 = new LocalizationPointDto()
            {
                PointId = 12,
                Point = new Point(new Position(52.549311, 19.713392)),
                StaticScore = 0
            };
 
            points.Add(plock);
            points.Add(ciolkowo);
            points.Add(sierpc);
            points.Add(torun);
            points.Add(ciechanow);
            points.Add(przasnysz);
            points.Add(lomza);
            points.Add(dzialdowo);
            points.Add(naruszewo);
            points.Add(rzewin);
            points.Add(plock2);
            points.Add(plock3);
        }

        public List<LocalizationPointDto> GetByParentId(long parentLocalizationPointId)
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

using DbModel;
using GeoJSON.Net.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Repositories
{
    public class MockLocalizationPointRepositoryMazowieckie : ILocalizationPointRepository
    {
        private List<LocalizationPoint> points = new List<LocalizationPoint>();

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

            LocalizationPoint plock = new LocalizationPoint()
            {
                PointId = 1,
                Point = new Point(new Position(52.546796, 19.708701)),
                StaticScore = 0
            };
            LocalizationPoint ciolkowo = new LocalizationPoint()
            {
                PointId = 2,
                Point = new Point(new Position(52.604040, 19.879803)),
                StaticScore = 0
            };
            LocalizationPoint sierpc = new LocalizationPoint()
            {
                PointId = 3,
                Point = new Point(new Position(52.855707, 19.668891)),
                StaticScore = 0
            };
            LocalizationPoint torun = new LocalizationPoint()
            {
                PointId = 4,
                Point = new Point(new Position(53.013152, 18.611620)),
                StaticScore = 0
            };
            LocalizationPoint ciechanow = new LocalizationPoint()
            {
                PointId = 5,
                Point = new Point(new Position(52.868006, 20.625774)),
                StaticScore = 0
            };
            LocalizationPoint przasnysz = new LocalizationPoint()
            {
                PointId = 6,
                Point = new Point(new Position(53.016507, 20.884518)),
                StaticScore = 0
            };

            LocalizationPoint lomza = new LocalizationPoint()
            {
                PointId = 7,
                Point = new Point(new Position(53.168437, 22.064546)),
                StaticScore = 0
            };
            LocalizationPoint dzialdowo = new LocalizationPoint()
            {
                PointId = 8,
                Point = new Point(new Position(53.229588, 20.167984)),
                StaticScore = 0
            };
            LocalizationPoint naruszewo = new LocalizationPoint()
            {
                PointId = 9,
                Point = new Point(new Position(52.525421, 20.351985)),
                StaticScore = 0
            };
            LocalizationPoint rzewin = new LocalizationPoint()
            {
                PointId = 10,
                Point = new Point(new Position(52.724651, 20.277417)),
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

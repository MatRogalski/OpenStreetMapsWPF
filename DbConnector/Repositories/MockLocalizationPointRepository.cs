using DbModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Repositories
{
    public class MockLocalizationPointRepository : ILocalizationPointRepository
    {
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
            throw new NotImplementedException();
        }
    }
}

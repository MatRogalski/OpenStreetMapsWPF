using DbModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Repositories
{
    public interface ILocalizationPointRepository
    {
		public List<LocalizationPoint> GetByParentId(long parentLocalizationPointId);

		public List<LocalizationPoint> GetWithAggregated();

		public List<LocalizationPoint> GetWithoutAggregated();
	}
}

using DbModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbConnector.Repositories
{
    public interface ILocalizationPointRepository
    {
		public List<LocalizationPointDto> GetByParentId(long parentLocalizationPointId);

		public List<LocalizationPointDto> GetWithAggregated();

		public List<LocalizationPointDto> GetWithoutAggregated();

		public List<LocalizationPointDto> GetAllPoints();
	}
}

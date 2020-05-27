using DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbConnector.Repositories
{
	public class LocalizationPointRepository : BaseRepository<LocalizationPoint>, ILocalizationPointRepository
	{
		public LocalizationPointRepository() : base()
		{

		}

		public List<LocalizationPoint> GetByParentId(long parentLocalizationPointId)
		{
			return this.dataSet.Where(i => i.ParentPointId == parentLocalizationPointId).ToList();
		}

		public List<LocalizationPoint> GetWithAggregated()
		{
			//this takes all normal points without aggregation ParentPointId == null and those which are aggregations. Points which are parts of aggregation meets condition ParentPointId != null
			return this.dataSet.Where(i => i.ParentPointId == null).ToList();
		}

		public List<LocalizationPoint> GetWithoutAggregated()
		{
			//this takes all normal points have static score == 0
			return this.dataSet.Where(i => i.StaticScore == 0).ToList();
		}
	}
}

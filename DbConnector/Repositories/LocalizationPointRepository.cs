using DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DbConnector.Repositories
{
	public class LocalizationPointRepository : BaseRepository<LocalizationPoint>
	{
		public LocalizationPointRepository() : base()
		{

		}

		public List<LocalizationPoint> GetByParentId(long parentLocalizationPointId)
		{
			return this.dataSet.Where(i => i.ParentPointId == parentLocalizationPointId).ToList();
		}
	}
}

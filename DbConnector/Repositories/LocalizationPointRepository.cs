using DbModel;
using Microsoft.EntityFrameworkCore;
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

		public LocalizationPointDto Add(LocalizationPointDto entity)
		{
			var entityToAdd = new LocalizationPoint(entity);
			this.dataSet.Add(entityToAdd);
			this.SaveChanges();

			return new LocalizationPointDto(entityToAdd);
		}
		
		public void Edit(LocalizationPointDto entity)
		{
			var entityToAdd = new LocalizationPoint(entity);
			this.context.Entry(entityToAdd).State = EntityState.Modified;
		}

		public bool Update(LocalizationPointDto entityDto)
		{

			LocalizationPoint dbPoint = this.dataSet.FirstOrDefault(i => i.PointId== entityDto.PointId);

			if (dbPoint != null)
			{
				dbPoint.GetDataFromLocalizationPointDto(entityDto);
				this.context.SaveChanges();
				return true;
			}
			return false;
		}
		
		public bool Update(LocalizationPoint entity)
		{

			LocalizationPoint dbPoint = this.dataSet.FirstOrDefault(i => i.PointId== entity.PointId);

			if (dbPoint != null)
			{
				dbPoint.GetDataFromLocalizationPoint(entity);
				this.context.SaveChanges();
				return true;
			}
			return false;
		}

		public List<LocalizationPointDto> GetByParentId(long parentLocalizationPointId)
		{
			return this.dataSet.Where(i => i.ParentPointId == parentLocalizationPointId).Select(i => new LocalizationPointDto(i)).ToList();
		}

		public List<LocalizationPointDto> GetWithAggregated()
		{
			//this takes all normal points without aggregation ParentPointId == null and those which are aggregations. Points which are parts of aggregation meets condition ParentPointId != null
			return this.dataSet.Where(i => i.ParentPointId == null).Select(i => new LocalizationPointDto(i)).ToList();
		}

		public List<LocalizationPointDto> GetWithoutAggregated()
		{
			//this takes all normal points have static score == 0
			return this.dataSet.Where(i => i.StaticScore == 0).Select(i=> new LocalizationPointDto(i)).ToList();
		}
	}
}

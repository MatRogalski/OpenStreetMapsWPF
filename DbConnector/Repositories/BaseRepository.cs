using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DbConnector.Repositories
{
	public abstract class BaseRepository <TEntity> where TEntity : class
	{
        protected readonly GeoDbContext context;
        protected readonly DbSet<TEntity> dataSet;

        public BaseRepository()
        {
            this.context = new GeoDbContext();
            this.dataSet = this.context.Set<TEntity>();
        }

        public virtual TEntity Add(TEntity entity)
        {
            return this.dataSet.Add(entity).Entity;
        }

        public virtual void Delete(TEntity entity)
        {
            this.dataSet.Remove(entity);
        }

        //check this out
        public virtual void Edit(TEntity entity)
        {
            this.context.Entry(entity).State = EntityState.Modified;
        }

        public virtual IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            return this.dataSet.Where(predicate);
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return this.context.Set<TEntity>();
        }

        public virtual void SaveChanges()
        {
            this.context.SaveChanges();
        }

    }
}

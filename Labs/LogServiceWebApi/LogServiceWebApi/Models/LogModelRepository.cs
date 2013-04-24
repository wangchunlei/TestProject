using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace LogServiceWebApi.Models
{ 
    public class LogModelRepository : ILogModelRepository
    {
        LogContext context = new LogContext();

        public IQueryable<LogModel> All
        {
            get { return context.LogModels; }
        }

        public IQueryable<LogModel> AllIncluding(params Expression<Func<LogModel, object>>[] includeProperties)
        {
            IQueryable<LogModel> query = context.LogModels;
            foreach (var includeProperty in includeProperties) {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public LogModel Find(System.Guid id)
        {
            return context.LogModels.Find(id);
        }

        public void InsertOrUpdate(LogModel logmodel)
        {
            if (logmodel.ID == default(System.Guid)) {
                // New entity
                logmodel.ID = Guid.NewGuid();
                context.LogModels.Add(logmodel);
            } else {
                // Existing entity
                context.Entry(logmodel).State = EntityState.Modified;
            }
            Save();
        }

        public void Delete(System.Guid id)
        {
            var logmodel = context.LogModels.Find(id);
            context.LogModels.Remove(logmodel);
            Save();
        }


        public void Save()
        {
            context.SaveChanges();
        }

        public void Dispose() 
        {
            context.Dispose();
        }
    }

    public interface ILogModelRepository : IDisposable
    {
        IQueryable<LogModel> All { get; }
        IQueryable<LogModel> AllIncluding(params Expression<Func<LogModel, object>>[] includeProperties);
        LogModel Find(System.Guid id);
        void InsertOrUpdate(LogModel logmodel);
        void Delete(System.Guid id);
        void Save();
    }
}
using Com.Gmlan.Middlewares.MongoDB.Interface;
using Com.Gmlan.Middlewares.MongoDB.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.Gmlan.Middlewares.MongoDB.Service
{
    public class MongoBusinessService<T> : IBusinessService<T> where T : MongoDocument
    {
        #region fields

        protected IRepository<T> Repository;
        #endregion

        #region ctor

        public MongoBusinessService(IRepository<T> repository)
        {
            Repository = repository;
        }

        #endregion
        public virtual void Delete(params T[] entities)
        {
            foreach (var entity in entities)
            {
                Repository.Delete(entity);
            }
        }

        public virtual T GetById(ObjectId id)
        {
            return Repository.GetById(id);
        }

        public virtual void Insert(params T[] entities)
        {
            foreach (var entity in entities)
            {
                Repository.Insert(entity);
            }
        }

        public virtual void Update(T entity)
        {
            Repository.Update(entity);
        }

        public IList<T> GetAll()
        {
            return Repository.GetAll();
        }

        public IList<T> GetByFilter(string field, string value)
        {
            return Repository.GetByFilter(field, value);
        }

        public Task InsertAsync(T entity)
        {
            return Repository.InsertAsync(entity);
        }

        public Task<ReplaceOneResult> UpdateAsync(T entity)
        {
            return Repository.UpdateAsync(entity);
        }

        public Task<DeleteResult> DeleteAsync(T entity)
        {
            return Repository.DeleteAsync(entity);
        }
    }
}

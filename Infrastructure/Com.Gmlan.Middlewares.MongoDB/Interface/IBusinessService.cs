using Com.Gmlan.Middlewares.MongoDB.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.Gmlan.Middlewares.MongoDB.Interface
{
    public interface IBusinessService<T> where T : MongoDocument
    {

        T GetById(ObjectId id);

        void Insert(params T[] entities);

        void Update(T entity);

        void Delete(params T[] entities);

        /// <summary>
        /// Get all documents
        /// </summary>
        /// <returns></returns>
        IList<T> GetAll();

        IList<T> GetByFilter(string field, string value);

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        Task InsertAsync(T entity);


        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        Task<ReplaceOneResult> UpdateAsync(T entity);


        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        Task<DeleteResult> DeleteAsync(T entity);

    }
}

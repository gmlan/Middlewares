using Com.Gmlan.Middlewares.MongoDB.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.Gmlan.Middlewares.MongoDB.Interface
{
    public interface IRepository<T> where T : MongoDocument
    {
        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        T GetById(ObjectId id);

        /// <summary>
        ///    Get entity list by filter
        /// </summary>
        /// <returns></returns>
        IList<T> GetByFilter(string field, string value);

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Insert(T entity);


        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Update(T entity);


        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        void Delete(T entity);

        /// <summary>
        /// Get all documents
        /// </summary>
        /// <returns></returns>
        IList<T> GetAll();

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

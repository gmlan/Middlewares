using Com.Gmlan.Middlewares.MongoDB.Attributes;
using Com.Gmlan.Middlewares.MongoDB.Interface;
using Com.Gmlan.Middlewares.MongoDB.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.Gmlan.Middlewares.MongoDB.Data
{
    public class MongoRepository<T> : IRepository<T> where T : MongoDocument
    {
        #region Fields
        private readonly MongoClient _client;
        private IMongoDatabase _db;
        private readonly string _collectionName;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public MongoRepository(MongoDbConfig config)
        {
            _client = new MongoClient(config.ConnectionString);
            _db = _client.GetDatabase(config.Database);

            _collectionName = typeof(T).Name;
            var collection = _db.GetCollection<T>(_collectionName);
            foreach (var p in typeof(T).GetProperties())
            {
                if (p.GetCustomAttributes(false).FirstOrDefault(m => m.GetType() == typeof(MongoIndex)) != null)
                {
                    collection.Indexes.CreateOneAsync(Builders<T>.IndexKeys.Text(new StringFieldDefinition<T>(p.Name)));
                }
            }
        }

        #endregion

        #region Utility

        private void NullableCheck(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
        }
        #endregion

        #region Methods

        public virtual IList<T> GetAll()
        {
            return _db.GetCollection<T>(_collectionName).Find(FilterDefinition<T>.Empty).ToList();
        }


        /// <summary>
        /// Get entity by identifier
        /// </summary>
        /// <param name="id">Identifier</param>
        /// <returns>Entity</returns>
        public virtual T GetById(ObjectId id)
        {

            return _db.GetCollection<T>(_collectionName).Find(Builders<T>.Filter.Eq("Id", id)).Single();
        }

        public IList<T> GetByFilter(string field, string value)
        {
            return _db.GetCollection<T>(_collectionName).Find(Builders<T>.Filter.Eq(field, value)).ToList();
        }

        /// <summary>
        /// Insert entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Insert(T entity)
        {
            NullableCheck(entity);
            _db.GetCollection<T>(_collectionName).InsertOne(entity);
        }
        public Task InsertAsync(T entity)
        {
            NullableCheck(entity);
            return _db.GetCollection<T>(_collectionName).InsertOneAsync(entity);
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Update(T entity)
        {
            NullableCheck(entity);
            _db.GetCollection<T>(_collectionName).ReplaceOne(x => x.Id == entity.Id, entity);

        }


        public Task<ReplaceOneResult> UpdateAsync(T entity)
        {
            NullableCheck(entity);
            return _db.GetCollection<T>(_collectionName).ReplaceOneAsync(x => x.Id == entity.Id, entity);
        }


        /// <summary>
        /// Delete entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Delete(T entity)
        {
            NullableCheck(entity);
            _db.GetCollection<T>(_collectionName).DeleteOne(x => x.Id == entity.Id);
        }

        public Task<DeleteResult> DeleteAsync(T entity)
        {
            NullableCheck(entity);
            return _db.GetCollection<T>(_collectionName).DeleteOneAsync(x => x.Id == entity.Id);
        }


        #endregion
    }
}


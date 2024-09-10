using ChatSupport.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace ChatSupport.Data
{
    public class MongoRepository<T> : IMongoRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database)
        {
            var collectionNameAttribute = typeof(T).GetCustomAttribute<CollectionNameAttribute>();

            if (collectionNameAttribute == null)
            {
                throw new Exception("CollectionNameAttribute is missing");
            }

            var collectionName = collectionNameAttribute.CollectionName;

            _collection = database.GetCollection<T>(collectionName);
        }

        public IList<T> GetAll()
        {
            return _collection.Find(Builders<T>.Filter.Empty).ToList();
        }

        public T GetById(ObjectId id)
        {
            return _collection.Find(Builders<T>.Filter.Eq("_id", id)).FirstOrDefault();
        }

        public async Task<bool> ExistsAsync(ObjectId id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("_id", id)).AnyAsync();
        }

        public IList<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return _collection
                        .Find(predicate)
                        .ToList();
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null || _collection == null)
            {
                throw new ArgumentNullException();
            }

            return _collection
                        .AsQueryable<T>()
                        .Where(predicate)
                        .First();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<int> CountAsync()
        {
            return (int)await _collection.CountDocumentsAsync(new BsonDocument());
        }

        public async Task UpdateAsync(ObjectId id, T entity)
        {
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", id), entity);
        }

        public async Task DeleteAsync(ObjectId id)
        {
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", id));
        }

        public async Task DeleteAllAsync()
        {
            await _collection.DeleteManyAsync(new BsonDocument());
        }
    }
}
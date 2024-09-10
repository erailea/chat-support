using System.Linq.Expressions;
using MongoDB.Bson;

namespace ChatSupport.Data
{
    public interface IMongoRepository<T> where T : class
    {
        IList<T> GetAll();
        T GetById(ObjectId id);
        IList<T> GetAll(Expression<Func<T, bool>> predicate);
        T Get(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(ObjectId id);
        Task<T> AddAsync(T entity);
        Task<int> CountAsync();
        Task UpdateAsync(ObjectId id, T entity);
        Task DeleteAsync(ObjectId id);
        Task DeleteAllAsync();
    }
}
using System.Linq.Expressions;

namespace MasCleaners.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null);

        Task AddAsync(T entity);

        Task AddRangeAsync(IEnumerable<T> entities);

        Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null);

        Task UpdateAsync(T entity);

        Task RemoveAsync(T entity);

        Task DetachEntityAsync(T entity);
    }
}

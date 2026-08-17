using MasCleaners.DataConnection;
using MasCleaners.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MasCleaners.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly SqlDbContext _context;
        internal DbSet<T> _dbSet;

        public Repository(SqlDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // ===== ADD =====
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // ===== ANY =====
        public async Task<bool> AnyAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter == null)
                return await _dbSet.AnyAsync();

            return await _dbSet.AnyAsync(filter);
        }

        // ===== GET SINGLE =====
        public async Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null
        )
        {
            IQueryable<T> query = _dbSet;

            query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        // ===== GET ALL =====
        public async Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null
        )
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
                query = query.Where(filter);

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            return await query.ToListAsync();
        }

        // ===== UPDATE =====
        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        // ===== REMOVE =====
        public Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        // ===== DETACH =====
        public Task DetachEntityAsync(T entity)
        {
            var entry = _context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }

            return Task.CompletedTask;
        }
    }
}

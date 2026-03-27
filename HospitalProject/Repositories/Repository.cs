using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using HospitalProject.Data;

namespace HospitalProject.Repositories
{
    // =========================
    // GENERIC INTERFACE
    // =========================
    public interface IRepository<T> where T : class
    {
        Task AddAsync(T entity);
        void Remove(T entity);          // 🔥 ADD THIS
        Task SaveAsync();
        IQueryable<T> Query();
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

        Task AddRangeAsync(IEnumerable<T> entities);
    }

    // =========================
    // GENERIC IMPLEMENTATION
    // =========================
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add new record
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }


        // 🔥 ADD THIS
        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        // Save changes
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Query (Include, Where use panna)
        public IQueryable<T> Query()
        {
            return _context.Set<T>();
        }

        // Get single record
        public async Task<T?> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }
    }
}

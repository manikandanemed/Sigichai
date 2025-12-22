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
        Task SaveAsync();
        IQueryable<T> Query();
        Task<T?> GetAsync(Expression<Func<T, bool>> predicate);
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
    }
}

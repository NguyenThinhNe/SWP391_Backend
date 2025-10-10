using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarrantyManagement.DAL.Data.Context;
using WarrantyManagement.DAL.Repositories.Interfaces;

namespace WarrantyManagement.DAL.Repositories.Implements
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly WarrantyDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public GenericRepository(WarrantyDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public virtual async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public virtual async Task<T> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public virtual void Update(T entity) => _dbSet.Update(entity);

        public virtual void Delete(T entity) => _dbSet.Remove(entity);
    }
}

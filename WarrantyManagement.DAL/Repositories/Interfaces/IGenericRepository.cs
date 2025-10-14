using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WarrantyManagement.DAL.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(Guid id);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(Guid id);

        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);

        Task Update(T entity);
        Task UpdateRange(IEnumerable<T> entities);

        Task<bool> Delete(Guid id);
        Task<bool> DeleteRange(IEnumerable<T> entities);
    }
}


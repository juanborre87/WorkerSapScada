using Arq.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Arq.Cqrs
{
    public class QuerySqlDb<T>(IDbContextProvider dbContextProvider) : IQuerySqlDb<T> where T : class
    {

        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filterExpression, string databaseChoice, bool tracking = true)
        {
            try
            {
                var _entity = dbContextProvider.GetDbSet<T>(databaseChoice);
                return tracking ? await _entity.FirstOrDefaultAsync(filterExpression) :
                    await _entity.AsNoTracking().FirstOrDefaultAsync(filterExpression);
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task<T> FirstOrDefaultIncludeAsync(string navigationPropertyPath, Expression<Func<T, bool>> filter, string databaseChoice, bool tracking = true)
        {
            try
            {
                var _entity = dbContextProvider.GetDbSet<T>(databaseChoice);
                return tracking ? await _entity.Include(navigationPropertyPath).FirstOrDefaultAsync(filter) :
                    await _entity.Include(navigationPropertyPath).AsNoTracking().FirstOrDefaultAsync(filter);
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task<List<T>> WhereAsync(Expression<Func<T, bool>> filterExpression, string databaseChoice, bool tracking = true)
        {
            try
            {
                var _entity = dbContextProvider.GetDbSet<T>(databaseChoice);
                return tracking ? await _entity.Where(filterExpression).ToListAsync() :
                    await _entity.AsNoTracking().Where(filterExpression).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task<List<T>> WhereIncludeAsync(string navigationPropertyPath, Expression<Func<T, bool>> filter, string databaseChoice, bool tracking = true)
        {
            try
            {
                var _entity = dbContextProvider.GetDbSet<T>(databaseChoice);
                return tracking ? await _entity.Include(navigationPropertyPath).Where(filter).ToListAsync() :
                    await _entity.Include(navigationPropertyPath).AsNoTracking().Where(filter).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

    }
}

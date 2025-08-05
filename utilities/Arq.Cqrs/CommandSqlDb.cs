using Arq.Core;
using Microsoft.EntityFrameworkCore;

namespace Arq.Cqrs
{
    public class CommandSqlDb<T>(IDbContextProvider dbContextProvider) : ICommandSqlDb<T> where T : class
    {
        public virtual async Task<T> AddAsync(T entity, string databaseChoice)
        {
            try
            {
                var dbSet = dbContextProvider.GetDbSet<T>(databaseChoice);
                await dbSet.AddAsync(entity);
                await dbContextProvider.SaveChangesAsync(databaseChoice);
                return entity;
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task DeleteAsync(T entity, string databaseChoice)
        {
            try
            {
                var entry = dbContextProvider.GetDbSet<T>(databaseChoice).Remove(entity);
                await dbContextProvider.SaveChangesAsync(databaseChoice);
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task UpdateAsync(T entity, string databaseChoice)
        {
            try
            {
                var entry = dbContextProvider.GetDbSet<T>(databaseChoice).Remove(entity);
                entry.State = EntityState.Modified;
                await dbContextProvider.SaveChangesAsync(databaseChoice);
            }
            catch
            {
                throw;
            }
        }

    }
}

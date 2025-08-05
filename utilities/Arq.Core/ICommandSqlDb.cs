namespace Arq.Core
{
    public interface ICommandSqlDb<T> where T : class
    {
        Task<T> AddAsync(T entity, string databaseChoice);
        Task DeleteAsync(T entity, string databaseChoice);
        Task UpdateAsync(T entity, string databaseChoice);
    }
}

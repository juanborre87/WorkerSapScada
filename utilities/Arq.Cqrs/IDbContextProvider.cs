using Microsoft.EntityFrameworkCore;

namespace Arq.Cqrs
{
    public interface IDbContextProvider
    {
        DbContext GetContext(string name);
        DbSet<T> GetDbSet<T>(string name) where T : class;
        Task SaveChangesAsync(string name);
    }
}

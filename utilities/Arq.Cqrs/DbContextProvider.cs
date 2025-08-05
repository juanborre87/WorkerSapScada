using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Arq.Cqrs
{
    public class DbContextProvider : IDbContextProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Func<IServiceProvider, DbContext>> _factories;

        public DbContextProvider(
            IServiceProvider serviceProvider,
            Dictionary<string, Func<IServiceProvider, DbContext>> factories)
        {
            _serviceProvider = serviceProvider;
            _factories = factories;
        }

        public DbContext GetContext(string name)
        {
            if (!_factories.TryGetValue(name, out var factory))
                throw new ArgumentException($"No se encontró un DbContext registrado con el nombre: {name}");

            var scope = _serviceProvider.CreateScope();
            return factory(scope.ServiceProvider);
        }

        public DbSet<T> GetDbSet<T>(string name) where T : class =>
            GetContext(name).Set<T>();

        public async Task SaveChangesAsync(string name) =>
            await GetContext(name).SaveChangesAsync();
    }
}

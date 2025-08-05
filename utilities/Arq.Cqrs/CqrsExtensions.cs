using Arq.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Arq.Cqrs
{
    public static class CqrsExtensions
    {
        public static IServiceCollection AddCQRS(this IServiceCollection services, Action<DbContextProviderBuilder> configure)
        {
            var builder = new DbContextProviderBuilder();
            configure(builder);
            var factories = builder.Build();

            services.AddSingleton<IDbContextProvider>(provider =>
                new DbContextProvider(provider, factories));

            services.AddScoped(typeof(ICommandSqlDb<>), typeof(CommandSqlDb<>));
            services.AddScoped(typeof(IQuerySqlDb<>), typeof(QuerySqlDb<>));

            return services;
        }
    }
}

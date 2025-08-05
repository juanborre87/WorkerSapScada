using Application.Interfaces;
using Arq.Core;
using Arq.Cqrs;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SapScadaMainDbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("SapScadaMain")),
            ServiceLifetime.Transient);
        services.AddDbContext<SapScada1DbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("SapScada1")),
            ServiceLifetime.Transient);
        services.AddDbContext<SapScada2DbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("SapScada2")),
            ServiceLifetime.Transient);
        services.AddHttpClient<SapOrderService>();
        services.AddTransient<ISapOrderService, SapOrderService>();

        services.AddCQRS(builder =>
        {
            builder.AddContext<SapScadaMainDbContext>("SapScadaMain");
            builder.AddContext<SapScada1DbContext>("SapScada1");
            builder.AddContext<SapScada2DbContext>("SapScada2");
        });

        //services.AddTransient<ICommandSqlDB<SolicitudPagoEntity>, CommandSqlDB<SolicitudPagoEntity>>();
        //services.AddTransient<IQuerySqlDB<SolicitudPagoEntity>, QuerySqlDB<SolicitudPagoEntity>>();
        services.AddTransient(typeof(ICommandSqlDb<>), typeof(CommandSqlDb<>));
        services.AddTransient(typeof(IQuerySqlDb<>), typeof(QuerySqlDb<>));


        return services;
    }

}
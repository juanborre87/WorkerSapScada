using Application.Interfaces;
using Arq.Core;
using Arq.Cqrs;
using Arq.Cqrs.Extensions;
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
            ServiceLifetime.Scoped);
        //services.AddDbContext<SapScada1DbContext>(
        //    options => options.UseSqlServer(configuration.GetConnectionString("SapScada1")),
        //    ServiceLifetime.Scoped);
        services.AddDbContext<SapScada2DbContext>(
            options => options.UseSqlServer(configuration.GetConnectionString("SapScada2")),
            ServiceLifetime.Scoped);
        services.AddHttpClient<SendConfirmSapService>();
        services.AddTransient<ISendConfirmSapService, SendConfirmSapService>();

        services.AddCQRS(builder =>
        {
            builder.AddContext<SapScadaMainDbContext>("SapScadaMain");
            //builder.AddContext<SapScada1DbContext>("SapScada1");
            builder.AddContext<SapScada2DbContext>("SapScada2");
        });

        services.AddSingleton<IFileLogger, FileLogger>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IEFCommandRepository<>), typeof(EFCommandRepository<>));
        services.AddScoped(typeof(IEFQueryRepository<>), typeof(EFQueryRepository<>));


        return services;
    }

}
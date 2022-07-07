using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Turnit.GenericStore.Infrastructure.Database.Mappings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Turnit.GenericStore.Infrastructure.Database;

public static class DependencyInjection
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(CreateSessionFactory);
        services.AddTransient(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());
        
        ISessionFactory CreateSessionFactory(IServiceProvider context)
        {
            var connectionString = configuration.GetConnectionString("Default");
            var dbConfiguration = Fluently.Configure()
                .Database(PostgreSQLConfiguration.PostgreSQL82
                    .Dialect<NHibernate.Dialect.PostgreSQL82Dialect>()
                    .ConnectionString(connectionString))
                .Mappings(x =>
                {
                    x.FluentMappings.AddFromAssemblyOf<ProductMap>();
                });

            return dbConfiguration.BuildSessionFactory();
        }
    }
}
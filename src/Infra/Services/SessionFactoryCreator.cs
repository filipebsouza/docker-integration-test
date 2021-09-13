using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace Infra.Services
{
    public class SessionFactoryCreator
    {
        public static ISessionFactory Create(string username, string password, string databaseName, string port)
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012
                    .ConnectionString(c => {
                        c.Username(username);
                        c.Password(password);
                        c.Database(databaseName);
                        c.Server($"localhost,{port}");
                    }))
                .BuildSessionFactory();
        }
    }
}
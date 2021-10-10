using System.Reflection;
using DbUp;

namespace Infra.Migrations
{
    public class MigrationRunner
    {
        public static bool Run(string connectionString)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader = DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetAssembly(typeof(MigrationRunner)))
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful) throw result.Error;

            return true;
        }
    }
}
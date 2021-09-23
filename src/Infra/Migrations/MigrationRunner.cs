using System;
using System.Reflection;
using DbUp;

namespace Infra.Migrations
{
    public class MigrationRunner
    {
        public static bool Run(string connectionString)
        {
            // DropDatabase.For.SqlDatabase(connectionString);
            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader = DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetAssembly(typeof(MigrationRunner)))
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
#if DEBUG
                Console.ReadLine();
#endif
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();

            return true;
        }
    }
}
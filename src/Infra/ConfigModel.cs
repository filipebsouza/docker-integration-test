namespace Infra
{
    public static class ConfigModel
    {
        public const string SqlServerImage = "mcr.microsoft.com/mssql/server";
        public const string ImageTag = "2017-latest-ubuntu";
        public const string VolumeName = "sql-server-volume";
        public const string ContainerName = "sql-server-for-tests";
        public const string ContainerExposedPort = "1433";
        public const string ContainerHostPort = "1401";
        public const string SqlServerDatabaseName = "master";
        public const string SqlServerUsername = "sa";
        public const string SqlServerPassword = "passW0rdSUPERh4rD";
        public static string[] ContainerEnvVariables => new[]
        {
            "ACCEPT_EULA=Y",
            $"SA_PASSWORD={SqlServerPassword}",
            "MSSQL_PID=Developer",
            "MSSQL_COLLATION=Latin1_General_CI_AI",
            "MSSQL_AGENT_ENABLED=true"
        };
        public static string ConnectionString = $"Server=localhost,{ContainerHostPort};Database={SqlServerDatabaseName};User Id={SqlServerUsername};Password={SqlServerPassword};";
    }
}
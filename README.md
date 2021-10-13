# Problem description:

If you need to test SQL Server queries/commands could be memory database doesn't have the necessary toolset for test specific SQL Server language (TOP 1, COLLATE, etc). Use mocks to simulate the behavior can doesn't be the expected result.

# Solution:

As an option, it's possible to use SQL Server on Docker container for integration tests. With them, it's possible to use a more realistic scenario like a production environment.

# Requirements:

- .NET 5 SDK;
- Docker (if you use Windows, run Docker on WSL2);

# Running the code:

Clone the repo and execute the following commands:
```console
dotnet restore
dotnet build
dotnet test src/IntegrationTests/IntegrationTests.csproj
```
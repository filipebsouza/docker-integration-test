using System;
using System.Threading.Tasks;
using Infra;
using Infra.Migrations;
using Infra.Models;
using Infra.Repositories;
using IntegrationTests.Helpers;
using Xunit;

namespace IntegrationTests
{
    public class ClientTests : IDisposable
    {
        private readonly SqlServerDockerManager _sqlServerDockerManager;
        private ClientRepository _clientRepository;

        public ClientTests()
        {
            _sqlServerDockerManager = new SqlServerDockerManager()
                .WithContainerName(ConfigModel.ContainerName)
                .WithExposedPort(ConfigModel.ContainerExposedPort)
                .WithHostPort(ConfigModel.ContainerHostPort)
                .WithImageName(ConfigModel.SqlServerImage)
                .WithTag(ConfigModel.ImageTag)
                .WithEnv(ConfigModel.ContainerEnvVariables)
                .WithVolumeName(ConfigModel.VolumeName)
                .WithConnectionString(ConfigModel.ConnectionString);
        }

        public void Dispose()
        {
            _sqlServerDockerManager.Dispose();
        }

        [Fact]
        public async Task Should_be_create_an_client_on_database()
        {
            await _sqlServerDockerManager.RunContainer();
            MigrationRunner.Run(ConfigModel.ConnectionString);
            _clientRepository = new ClientRepository(ConfigModel.ConnectionString);
            var client = new Client
            {
                Name = "John Carter",
                Age = 34,
                Active = true
            };

            _clientRepository.Insert(client);

            var clients = _clientRepository.GetAll();
            Assert.Contains(clients, c => c.Name == client.Name && c.Age == client.Age);
        }
    }
}

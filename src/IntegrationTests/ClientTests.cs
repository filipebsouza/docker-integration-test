using System;
using System.Threading.Tasks;
using Infra;
using Infra.Models;
using Infra.Repositories;
using Infra.Services;
using IntegrationTests.Helpers;
using NHibernate;
using Xunit;

namespace IntegrationTests
{
    public class ClientTests : IAsyncDisposable
    {
        private readonly SqlServerDockerManager _sqlServerDockerManager;
        private ISession _session;
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
                .WithVolumeName(ConfigModel.VolumeName);            
        }

        public async ValueTask DisposeAsync()
        {
            _session.Close();
            await _sqlServerDockerManager.DisposeAsync();
        }

        [Fact]
        public async Task Should_be_create_an_client_on_database()
        {
            await _sqlServerDockerManager.RunContainer();
            _session = SessionFactoryCreator.Create(ConfigModel.SqlServerUsername,
                ConfigModel.SqlServerPassword, ConfigModel.SqlServerDatabaseName,
                ConfigModel.ContainerHostPort).OpenSession();
            _clientRepository = new ClientRepository(_session);
            var client = new Client
            {
                Name = "John Carter",
                Age = 34,
                Active = true
            };
            var transaction = _session.BeginTransaction();

            _clientRepository.Insert(client);
            transaction.Commit();

            var clients = _clientRepository.GetAll();
            Assert.Contains(clients, c => c.Name == client.Name && c.Age == client.Age);
        }
    }
}

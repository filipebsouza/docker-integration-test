using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace IntegrationTests.Helpers
{
    public class SqlServerDockerManager : IDisposable
    {
        private readonly string _dockerUri =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "npipe://./pipe/docker_engine" :
                "unix:///var/run/docker.sock";
        private readonly DockerClient _dockerClient;
        private string _containerID = string.Empty;

        private string VolumeName { get; set; }
        private string ImageName { get; set; }
        private string ExposedPort { get; set; }
        private string Tag { get; set; }
        private string HostPort { get; set; }
        private string ContainerName { get; set; }
        private string[] Env { get; set; }
        private string ConnectionString { get; set; }

        public SqlServerDockerManager()
        {
            _dockerClient = new DockerClientConfiguration(new Uri(_dockerUri)).CreateClient();
        }

        public SqlServerDockerManager WithVolumeName(string volumeName)
        {
            VolumeName = volumeName;
            return this;
        }

        public SqlServerDockerManager WithImageName(string imageName)
        {
            ImageName = imageName;
            return this;
        }

        public SqlServerDockerManager WithExposedPort(string exposedPort)
        {
            ExposedPort = exposedPort;
            return this;
        }

        public SqlServerDockerManager WithTag(string tag)
        {
            Tag = tag;
            return this;
        }

        public SqlServerDockerManager WithHostPort(string hostPort)
        {
            HostPort = hostPort;
            return this;
        }

        public SqlServerDockerManager WithContainerName(string containerName)
        {
            ContainerName = containerName;
            return this;
        }

        public SqlServerDockerManager WithEnv(string[] env)
        {
            Env = env;
            return this;
        }

        public SqlServerDockerManager WithConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
            return this;
        }

        public async Task RunContainer()
        {
            if (!string.IsNullOrWhiteSpace(_containerID)) return;

            ValidateRequiredProperties();

            await _dockerClient.Volumes.CreateAsync(new VolumesCreateParameters
            {
                Name = VolumeName
            });

            await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {
                FromImage = ImageName,
                Tag = $"{Tag ?? "latest"}"
            }, new AuthConfig
            {
                Email = "email@email.com",
                Password = "XXX",
                Username = "username"
            }, new Progress<JSONMessage>());

            var container = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = $"{ImageName}:{Tag ?? "latest"}",
                Name = ContainerName,
                Env = Env,
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {
                        ExposedPort, new EmptyStruct()
                    }
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            ExposedPort, new List<PortBinding>
                            {
                                new PortBinding
                                {
                                    HostIP = "localhost",
                                    HostPort = HostPort
                                }
                            }
                        }
                    },
                    Binds = new List<string>
                    {
                        $"{VolumeName}:/var/opt/mssql/data"
                    }
                }
            });

            _containerID = container.ID;

            await _dockerClient.Containers.StartContainerAsync(_containerID, null);
            await WaitUntilDatabaseAvailableAsync();
        }

        private async Task WaitUntilDatabaseAvailableAsync()
        {
            var start = DateTime.UtcNow;
            const int maxWaitTimeSeconds = 60;
            var connectionEstablised = false;
            while (!connectionEstablised && start.AddSeconds(maxWaitTimeSeconds) > DateTime.UtcNow)
            {
                try
                {
                    using var sqlConnection = new SqlConnection(ConnectionString);
                    await sqlConnection.OpenAsync();
                    connectionEstablised = true;
                }
                catch
                {
                    await Task.Delay(500);
                }
            }

            if (!connectionEstablised)
                throw new Exception("Connection to the SQL docker database could not be established within 60 seconds.");

            return;
        }

        private void ValidateRequiredProperties()
        {
            ThrowIfStringIsNullOrWhiteSpace(ImageName, nameof(ImageName));
            ThrowIfStringIsNullOrWhiteSpace(ExposedPort, nameof(ExposedPort));
            ThrowIfStringIsNullOrWhiteSpace(HostPort, nameof(HostPort));
            ThrowIfStringIsNullOrWhiteSpace(ContainerName, nameof(ContainerName));
            ThrowIfStringIsNullOrWhiteSpace(VolumeName, nameof(VolumeName));
            ThrowIfStringIsNullOrWhiteSpace(ConnectionString, nameof(ConnectionString));
        }

        private static void ThrowIfStringIsNullOrWhiteSpace(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{propertyName} not can be null or white space!");
        }

        public void Dispose()
        {
            if (_containerID is { Length: > 0 })
            {
                _dockerClient.Containers.RemoveContainerAsync(_containerID, new ContainerRemoveParameters
                {
                    Force = true
                }).ConfigureAwait(false);

                var sqlServerVolumeIsRemoved = false;
                do
                {
                    _dockerClient.Volumes.RemoveAsync(VolumeName, true, new CancellationToken()).ConfigureAwait(false);

                    var runningVolumes = _dockerClient.Volumes.ListAsync().Result;
                    sqlServerVolumeIsRemoved = !runningVolumes.Volumes.Any(v => v.Name == VolumeName);
                }
                while (!sqlServerVolumeIsRemoved);

                _dockerClient.Dispose();
            }
        }
    }
}
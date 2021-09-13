using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace IntegrationTests.Helpers
{
    public class SqlServerDockerManager : IAsyncDisposable
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
            }, null, new Progress<JSONMessage>());

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
        }

        private void ValidateRequiredProperties()
        {
            ThrowIfStringIsNullOrWhiteSpace(ImageName, nameof(ImageName));
            ThrowIfStringIsNullOrWhiteSpace(ExposedPort, nameof(ExposedPort));
            ThrowIfStringIsNullOrWhiteSpace(HostPort, nameof(HostPort));
            ThrowIfStringIsNullOrWhiteSpace(ContainerName, nameof(ContainerName));
            ThrowIfStringIsNullOrWhiteSpace(VolumeName, nameof(VolumeName));
        }

        private static void ThrowIfStringIsNullOrWhiteSpace(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{propertyName} not can be null or white space!");
        }

        public async ValueTask DisposeAsync()
        {
            if (!string.IsNullOrWhiteSpace(_containerID))
            {
                await _dockerClient.Containers.StopContainerAsync(_containerID, new ContainerStopParameters());
                await _dockerClient.Containers.RemoveContainerAsync(_containerID, new ContainerRemoveParameters());
                await _dockerClient.Volumes.RemoveAsync(VolumeName, true, new CancellationToken());
            }

            _dockerClient.Dispose();
        }
    }
}
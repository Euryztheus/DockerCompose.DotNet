using System.Globalization;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerComposeLiteAPI.src.structure;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace DockerComposeLiteAPI;

public class ComposeLite
{
  private readonly string composeFileString;
  private ComposeFileDefinition composeFile;
  private readonly DockerClient client;
  private readonly TextWriter log;
  private readonly string projectName;

  public ComposeLite(string composeFileString, string? projectName = "", TextWriter? log = null)
  {
    this.composeFileString = composeFileString;
    this.client = new DockerClientConfiguration(
        new Uri("unix:///var/run/docker.sock"))
         .CreateClient();

    this.log = log ?? Console.Out;
    this.projectName = projectName;
  }

  public void ParseComposeFile()
  {

    var input = new StringReader(composeFileString);
    var deserializer = new DeserializerBuilder().Build();
    var yamlObject = deserializer.Deserialize(input);

    var serializer = new SerializerBuilder()
        .JsonCompatible()
        .Build();

    var json = serializer.Serialize(yamlObject);

    //Console.WriteLine(json);
    composeFile = JsonConvert.DeserializeObject<ComposeFileDefinition>(json);
    /*/
    foreach (KeyValuePair<string, ServiceDefinition> ser in composeFile.Services)
    {
      Console.WriteLine(ser.Value);
    }//*/
  }

  public async Task ComposeUp()
  {
    foreach (KeyValuePair<string, ServiceDefinition> ser in composeFile.Services)
    {
      // check/create networks
      await CreateNetworks();
      // check/create volumes
      await CreateVolumes();
      // check/download images
      await GetImages();
      // create container
      await CreateContainer();
      // connect container to network


      log.WriteLine("ComposeLite: Compose up complete");
    }
  }

  public async Task ComposeDown()
  {
    // stop and remove containers
    await RemoveContainers();
    // remove networks
    await RemoveNetworks();
    // -v, --volumes remove volumes
    // --rmi remove images
  }

  public async Task RemoveContainers()
  {
    var existingContainers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true });
    foreach (var (name, service) in composeFile.Services)
    {
      var existingContainer = existingContainers.FirstOrDefault(c => c.Names.Any(n => n.TrimStart('/') == PrefixName(service.ContainerName)));
      if (existingContainer == null)
      {
        log.WriteLine($"Issue: Container {PrefixName(service.ContainerName)} does not exist!");
        continue;
      }
      log.WriteLine($"Stopping Container {PrefixName(service.ContainerName)}");
      await client.Containers.StopContainerAsync(existingContainer.ID, new ContainerStopParameters());
      log.WriteLine($"Container {PrefixName(service.ContainerName)} has been stopped");
      await client.Containers.RemoveContainerAsync(existingContainer.ID, new ContainerRemoveParameters());
      log.WriteLine($"Container {PrefixName(service.ContainerName)} has been removed");
    }
  }

  public async Task RemoveNetworks()
  {
    var existingNetworks = await client.Networks.ListNetworksAsync(new NetworksListParameters());
    var existingNetworksDict = existingNetworks.ToDictionary(n => n.Name, n => n);

    foreach (var (name, net) in composeFile.Networks)
    {
      if (!existingNetworksDict.TryGetValue(PrefixName(name), out var n))
      {
        log.WriteLine($"Issue: Network {PrefixName(name)} does not exist");
        continue;
      }
      // TODO: check if network is being used
      await client.Networks.DeleteNetworkAsync(PrefixName(name));
      log.WriteLine($"Network {PrefixName(name)} has been removed");
    }
  }

  public async Task RemoveVolumes()
  {

  }

  public async Task RemoveImages()
  {

  }

  public async Task CreateContainer()
  {
    var existingContainers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true });
    foreach (var (name, service) in composeFile.Services)
    {
      var existingContainer = existingContainers.FirstOrDefault(c => c.Names.Any(n => n.TrimStart('/') == PrefixName(service.ContainerName)));
      if (existingContainer != null)
      {
        log.WriteLine($"Container {PrefixName(service.ContainerName)} already exists");
        await client.Containers.StartContainerAsync(existingContainer.ID, new ContainerStartParameters());
        log.WriteLine($"Container {PrefixName(service.ContainerName)} has been started");
        continue;
      }

      // TODO: config (ports, sysctl)

      var newContainer = new CreateContainerParameters
      {
        Hostname = service.Hostname,
        Name = PrefixName(service.ContainerName),
        Entrypoint = new List<string> { service.Entrypoint ?? "" },
        User = service.User,
        Image = service.Image,
        Tty = service.Tty,
        Env = service.Environment,
        Cmd = service.Command,
        //depends on?
        // Volumes = // old api, use hostconfig instead 
      };

      // bind volumes
      var mounts = ParseVolumeToMount(service);
      newContainer.HostConfig = new HostConfig
      {
        Mounts = mounts.Count > 0 ? mounts : null
      };

      var created = await client.Containers.CreateContainerAsync(newContainer);
      log.WriteLine($"Container {PrefixName(service.ContainerName)} has been created");
      await client.Containers.StartContainerAsync(created.ID, new ContainerStartParameters());
      log.WriteLine($"Container {PrefixName(service.ContainerName)} has been started");
    }
  }

  public async Task GetImages()
  {
    foreach (var (name, service) in composeFile.Services)
    {
      string image = service.Image ?? "";
      //await client.Images.InspectImageAsync(image);
      await client.Images.CreateImageAsync(
        new ImagesCreateParameters { FromImage = image },
        authConfig: null,
        progress: new Progress<JSONMessage>(m =>
        {
          if (!string.IsNullOrWhiteSpace(m.Status))
            log.WriteLine($"{image}: {m.Status} {m.ProgressMessage}".Trim());
        })
      );
    }
  }

  public async Task CreateNetworks()
  {
    var existingNetworks = await client.Networks.ListNetworksAsync(new NetworksListParameters());
    var existingNetworksDict = existingNetworks.ToDictionary(n => n.Name, n => n);

    foreach (var (name, net) in composeFile.Networks)
    {
      if (existingNetworksDict.TryGetValue(PrefixName(name), out var n))
      {
        log.WriteLine($"Network {PrefixName(name)} already exists");
        continue;
      }

      //create network if not exist
      log.WriteLine($"Network {PrefixName(name)} will be created");
      var newNetwork = new NetworksCreateParameters
      {
        Name = PrefixName(name),
        Driver = net.Driver ?? "bridge",
        Attachable = net.Attachable ?? true,
        Options = net.DriverOpts
      };
      if (net.EnableIpv6.HasValue)
        newNetwork.EnableIPv6 = net.EnableIpv6.Value;

      if (net.Ipam?.Config is { Count: > 0 } pools)
      {
        newNetwork.IPAM = new IPAM
        {
          Config = pools.Select(p => new IPAMConfig { Subnet = p.Subnet }).ToList()
        };
      }

      await client.Networks.CreateNetworkAsync(newNetwork);
      log.WriteLine($"Network {PrefixName(name)} created successfully");
    }
  }

  public async Task CreateVolumes()
  {
    var existingVolumes = await client.Volumes.ListAsync(new VolumesListParameters());
    var existingVolumesDict = existingVolumes.Volumes.ToDictionary(n => n.Name, n => n);

    foreach (var (name, volDef) in composeFile.Volumes)
    {
      var vol = volDef ?? new VolumeDefinition();
      if (vol.External == true)
      {
        log.WriteLine($"Volume {PrefixName(name)} is external, wont create");
        continue;
      }
      if (existingVolumesDict.TryGetValue(PrefixName(name), out var v))
      {
        log.WriteLine($"Volume {PrefixName(name)} already exists, wont create");
        continue;
      }

      log.WriteLine($"Volume {PrefixName(name)} will be created");
      var newVolume = new VolumesCreateParameters
      {
        Name = PrefixName(name),
        Driver = vol.Driver,
        DriverOpts = vol.DriverOpts,
        Labels = vol.Labels
      };

      await client.Volumes.CreateAsync(newVolume);
      log.WriteLine($"Volume {PrefixName(name)} created successfully");
    }
  }


  // Helper Methods

  // Generated by ChatGPT, need review
  private List<Mount> ParseVolumeToMount(ServiceDefinition service)
  {
    var mounts = new List<Mount>();

    foreach (var v in service.Volumes ?? new List<string>())
    {
      if (string.IsNullOrWhiteSpace(v)) continue;

      // "source:target[:mode]"
      var parts = v.Split(':', StringSplitOptions.None);
      if (parts.Length < 2 || parts.Length > 3)
        throw new InvalidOperationException($"Invalid volume spec: {v}");

      var source = parts[0].Trim();
      var target = parts[1].Trim();
      var mode = parts.Length == 3 ? parts[2].Trim() : "rw";
      var readOnly = mode.Equals("ro", StringComparison.OrdinalIgnoreCase);

      var isBind = source.StartsWith("/", StringComparison.Ordinal); // Linux-only heuristic

      mounts.Add(new Mount
      {
        Type = isBind ? "bind" : "volume",
        Source = isBind ? source : PrefixName(source),  // prefix named volumes
        Target = target,
        ReadOnly = readOnly
      });
    }
    return mounts;
  }
  private string PrefixName(string? key = "")
  {
    return projectName + "_" + key;
  }
}

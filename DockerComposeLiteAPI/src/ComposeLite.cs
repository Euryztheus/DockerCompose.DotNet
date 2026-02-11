using System.Globalization;
using System.Threading.Tasks;
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
      CreateContainer();
      // connect container to network


      log.WriteLine("ComposeLite: Compose up complete");
    }
  }

  public async Task ComposeDown()
  {

  }

  public async Task CreateContainer()
  {

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
      if (existingNetworksDict.TryGetValue(prefixName(name), out var n))
      {
        log.WriteLine($"Network {prefixName(name)} already exists");
        continue;
      }

      //create network if not exist
      log.WriteLine($"Network {prefixName(name)} will be created");
      var newNetwork = new NetworksCreateParameters
      {
        Name = prefixName(name),
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
      log.WriteLine($"Network {prefixName(name)} created successfully");
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
        log.WriteLine($"Volume {prefixName(name)} is external, wont create");
        continue;
      }
      if (existingVolumesDict.TryGetValue(prefixName(name), out var v))
      {
        log.WriteLine($"Volume {prefixName(name)} already exists, wont create");
        continue;
      }

      log.WriteLine($"Volume {prefixName(name)} will be created");
      var newVolume = new VolumesCreateParameters
      {
        Name = prefixName(name),
        Driver = vol.Driver,
        DriverOpts = vol.DriverOpts,
        Labels = vol.Labels
      };

      await client.Volumes.CreateAsync(newVolume);
      log.WriteLine($"Volume {prefixName(name)} created successfully");
    }
  }


  // Helper Methods
  private string prefixName(string key)
  {
    return projectName + "_" + key;
  }
}

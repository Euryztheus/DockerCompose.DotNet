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
  public ComposeLite(string composeFileString, TextWriter? log = null)
  {
    this.composeFileString = composeFileString;
    this.client = new DockerClientConfiguration(
        new Uri("unix:///var/run/docker.sock"))
         .CreateClient();

    this.log = log ?? Console.Out;
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
      // create container
      // connect container to network
    }
  }

  public async Task ComposeDown()
  {

  }

  public async Task CreateNetworks()
  {
    var existingNetworks = await client.Networks.ListNetworksAsync(new NetworksListParameters());
    var existingNetworksDict = existingNetworks.ToDictionary(n => n.Name, n => n);

    foreach (var (name, net) in composeFile.Networks)
    {
      if (existingNetworksDict.TryGetValue(name, out var n))
      {
        log.WriteLine($"Network {name} already exists");
        continue;
      }

      //create network if not exist
      log.WriteLine($"Network {name} will be created");
      var newNetwork = new NetworksCreateParameters
      {
        Name = name,
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
      log.WriteLine($"Network {name} created successfully");
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
        log.WriteLine($"Volume {name} is external, wont create");
        continue;
      }
      if (existingVolumesDict.TryGetValue(name, out var v))
      {
        log.WriteLine($"Volume {name} already exists, wont create");
        continue;
      }

      log.WriteLine($"Volume {name} will be created");
      var newVolume = new VolumesCreateParameters
      {
        Name = name,
        Driver = vol.Driver,
        DriverOpts = vol.DriverOpts,
        Labels = vol.Labels
      };

      await client.Volumes.CreateAsync(newVolume);
      log.WriteLine($"Volume {name} created successfully");
    }
  }
}

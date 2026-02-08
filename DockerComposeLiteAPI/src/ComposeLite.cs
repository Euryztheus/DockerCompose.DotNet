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
  public ComposeLite(string composeFileString)
  {
    this.composeFileString = composeFileString;
    this.client = new DockerClientConfiguration(
        new Uri("unix:///var/run/docker.sock"))
         .CreateClient();
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
      await CreateNetwork();
      // check/create volumes
      // check/download images
      // create container
      // connect container to network
    }
  }

  public async Task CreateNetwork()
  {
    var existingNetworks = await client.Networks.ListNetworksAsync(new NetworksListParameters());
    var existingNetworksDict = existingNetworks.ToDictionary(n => n.Name, n => n);

    foreach (var (name, net) in composeFile.Networks)
    {
      if (existingNetworksDict.TryGetValue(name, out var n))
      {
        continue;
      }

      //create network if not exist
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

      Console.WriteLine($"creating network: {newNetwork.Name}");
      await client.Networks.CreateNetworkAsync(newNetwork);
    }
  }
}

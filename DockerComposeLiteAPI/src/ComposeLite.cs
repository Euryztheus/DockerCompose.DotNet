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
    try
    {
      var networks = await client.Networks.ListNetworksAsync(new NetworksListParameters());

      foreach (var item in networks)
      {
        Console.WriteLine(item.Name);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
      throw;
    }

    foreach (KeyValuePair<string, NetworkDefinition> net in composeFile.Networks)
    {
      Console.WriteLine();
    }
  }
}

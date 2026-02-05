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

  public void ComposeUp()
  {
    foreach (KeyValuePair<string, ServiceDefinition> ser in composeFile.Services)
    {
      // check/create networks
      CreateNetwork();
      // check/create volumes
      // check/download images
      // create container
      // connect container to network
    }
  }

  public async void CreateNetwork()
  {
    IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(new ContainersListParameters()
    {
      Limit = 10,
    });

    foreach (var item in containers)
    {
      Console.WriteLine(item);
    }

    foreach (KeyValuePair<string, NetworkDefinition> net in composeFile.Networks)
    {
      Console.WriteLine();
    }
  }
}

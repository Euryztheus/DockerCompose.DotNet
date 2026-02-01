using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace DockerComposeLiteAPI;

public class ComposeLiteAPI
{
  private string composeFile;
  public ComposeLiteAPI(string composeFile)
  {
    this.composeFile = composeFile;
  }

  public void ParseComposeFile()
  {

    var input = new StringReader(composeFile);
    var deserializer = new DeserializerBuilder().Build();
    var yamlObject = deserializer.Deserialize(input);

    var serializer = new SerializerBuilder()
        .JsonCompatible()
        .Build();

    var json = serializer.Serialize(yamlObject);

    //Console.WriteLine(json);
    Dictionary<string, object> cpFile = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
    foreach (var item in cpFile)
    {
      Console.WriteLine(item.Value);
    }

  }

}

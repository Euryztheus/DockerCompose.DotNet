using YamlDotNet.RepresentationModel;

namespace DockerComposeLiteAPI;

public class ComposeLiteAPI
{
  private string composeFile;
  public ComposeLiteAPI(string composeFile)
  {
    Console.WriteLine("working");
    this.composeFile = composeFile;
  }

  public void parseComposeFile()
  {

    var input = new StringReader(composeFile);
    var yaml = new YamlStream();
    yaml.Load(input);

    // Examine the stream
    var mapping =
        (YamlMappingNode)yaml.Documents[0].RootNode;

    foreach (var entry in mapping.Children)
    {
      Console.WriteLine(entry);
    }
  }

}

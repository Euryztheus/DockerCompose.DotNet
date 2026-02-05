using DockerComposeLiteAPI.src.structure;
using Newtonsoft.Json;

namespace DockerComposeLiteAPI.src.structure;

public class ComposeFileDefinition
{
  [JsonProperty("version")]
  public string? Version { get; set; }

  [JsonProperty("services")]
  public Dictionary<string, ServiceDefinition> Services { get; set; } = new();

  [JsonProperty("volumes")]
  public Dictionary<string, VolumeDefinition> Volumes { get; set; } = new();

  [JsonProperty("networks")]
  public Dictionary<string, NetworkDefinition> Networks { get; set; } = new();
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerComposeDotNet.src.structure;

public class VolumeDefinition
{
  [JsonProperty("driver")]
  public string? Driver { get; set; }

  [JsonProperty("driver_opts")]
  public Dictionary<string, string>? DriverOpts { get; set; }

  [JsonProperty("external")]
  public bool? External { get; set; }

  [JsonProperty("name")]
  public string? Name { get; set; }

  [JsonProperty("labels")]
  public Dictionary<string, string>? Labels { get; set; }
}
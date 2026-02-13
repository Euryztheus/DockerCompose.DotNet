using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerComposeDotNet.src.structure;

public class IpamConfig
{
  [JsonProperty("driver")]
  public string? Driver { get; set; }

  [JsonProperty("config")]
  public List<IpamPool>? Config { get; set; }
}
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerComposeDotNet.src.structure;

public class NetworkDefinition
{
  [JsonProperty("driver")]
  public string? Driver { get; set; }

  [JsonProperty("attachable")]
  public bool? Attachable { get; set; }

  [JsonProperty("enable_ipv6")]
  public bool? EnableIpv6 { get; set; }

  [JsonProperty("internal")]
  public bool? Internal { get; set; }

  [JsonProperty("external")]
  public bool? External { get; set; }

  [JsonProperty("name")]
  public string? Name { get; set; }

  [JsonProperty("labels")]
  public Dictionary<string, string>? Labels { get; set; }

  [JsonProperty("driver_opts")]
  public Dictionary<string, string>? DriverOpts { get; set; }

  [JsonProperty("ipam")]
  public IpamConfig? Ipam { get; set; }
}

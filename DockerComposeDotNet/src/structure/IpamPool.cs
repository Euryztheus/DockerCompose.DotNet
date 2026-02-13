using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerComposeDotNet.src.structure;

public class IpamPool
{
  [JsonProperty("subnet")]
  public string? Subnet { get; set; }

  [JsonProperty("ip_range")]
  public string? IpRange { get; set; }

  [JsonProperty("gateway")]
  public string? Gateway { get; set; }

  [JsonProperty("aux_addresses")]
  public Dictionary<string, string>? AuxAddresses { get; set; }
}
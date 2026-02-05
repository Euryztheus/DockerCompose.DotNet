using System.Collections.Generic;
using Newtonsoft.Json;

namespace DockerComposeLiteAPI.src.structure;

public class ServiceDefinition
{
  [JsonProperty("container_name")]
  public string? ContainerName { get; set; }

  [JsonProperty("hostname")]
  public string? Hostname { get; set; }

  [JsonProperty("entrypoint")]
  public string? Entrypoint { get; set; }

  [JsonProperty("user")]
  public string? User { get; set; }

  [JsonProperty("image")]
  public string? Image { get; set; }

  [JsonProperty("tty")]
  public bool Tty { get; set; }

  [JsonProperty("environment")]
  public List<string> Environment { get; set; } = new();

  [JsonProperty("ports")]
  public List<string> Ports { get; set; } = new();

  [JsonProperty("depends_on")]
  public List<string> DependsOn { get; set; } = new();

  [JsonProperty("volumes")]
  public List<string> Volumes { get; set; } = new();

  [JsonProperty("networks")]
  public List<string> Networks { get; set; } = new();

  [JsonProperty("sysctls")]
  public List<string> Sysctls { get; set; } = new();

  public override string ToString()
  {
    var lines = new List<string>();

    void Add(string label, string? value)
    {
      if (!string.IsNullOrWhiteSpace(value))
        lines.Add($"{label}: {value}");
    }

    void AddList(string label, List<string> values)
    {
      if (values is { Count: > 0 })
        lines.Add($"{label}: {string.Join(", ", values)}");
    }

    Add("Container Name", ContainerName);
    Add("Hostname", Hostname);
    Add("Entrypoint", Entrypoint);
    Add("User", User);
    Add("Image", Image);

    // bool is meaningful only if explicitly enabled
    if (Tty)
      lines.Add("TTY: true");

    AddList("Environment", Environment);
    AddList("Ports", Ports);
    AddList("Depends On", DependsOn);
    AddList("Volumes", Volumes);
    AddList("Networks", Networks);
    AddList("Sysctls", Sysctls);

    return lines.Count == 0
      ? "<empty service>"
      : string.Join('\n', lines);
  }
}


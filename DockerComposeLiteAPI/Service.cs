using System.Collections.Generic;

namespace DockerComposeLiteAPI;

public class Service
{
    public string? containerName;
    public string? hostname;
    public string? entrypoint;
    public string? user;
    public string? image;
    public bool tty;
    public List<string> environment = new();
    public List<string> ports = new();
    public List<Service> dependsOn = new();
    public List<string> volumes = new();
    public List<string> networks = new();
    public List<string> sysctls = new(); // Linux kernel sysctl settings (e.g., "net.core.somaxconn=1024")

    public override string ToString()
    {
        static string JoinOrDash(List<string>? items) =>
          items is null || items.Count == 0 ? "-" : string.Join(", ", items);

        string DependsOrDash(List<Service>? items)
        {
            if (items is null || items.Count == 0) return "-";
            return string.Join(", ", items.ConvertAll(s => s.containerName ?? s.image ?? s.hostname ?? "<service>"));
        }

        return
          $"Container Name: {containerName ?? "-"}\n" +
          $"Hostname: {hostname ?? "-"}\n" +
          $"Entrypoint: {entrypoint ?? "-"}\n" +
          $"User: {user ?? "-"}\n" +
          $"Image: {image ?? "-"}\n" +
          $"TTY: {tty}\n" +
          $"Environment: {JoinOrDash(environment)}\n" +
          $"Ports: {JoinOrDash(ports)}\n" +
          $"Depends On: {DependsOrDash(dependsOn)}\n" +
          $"Volumes: {JoinOrDash(volumes)}\n" +
          $"Networks: {JoinOrDash(networks)}\n" +
          $"Sysctls: {JoinOrDash(sysctls)}";
    }
}

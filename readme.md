# DockerComposeDotNet

Minimal .NET library that parses a docker-compose.yml string and runs a limited “compose up/down” against the local Docker Engine via Docker.DotNet.

Developed for my DockerGUI Program. Will receive new features/updates when required

## Usage
```c#
using DockerComposeDotNet;

var yaml = File.ReadAllText("docker-compose.yml");

var dcdn = new DockerComposeDotNet(
  yaml,
  projectName: "demo",
  log: Console.Out
);
// alternative
// var dcdn = new DockerComposeDotNet(yaml);

dcdn.ParseComposeFile();
await dcdn.ComposeUp();
// await dcdn.ComposeDown();
```

## Limitations

- Linux-only Docker connection (hardcoded socket)
- entrypoint list not supported (["/bin/sh","-c", "..."] won’t work); only a single string: "/bin/bash -c 'echo hi'"
- compose down volumes/images removal not yet implemented
- No depends_on, build, restart policies, healthchecks, per-service networks, secrets/configs, etc.
- binding only works with linux and absolute paths


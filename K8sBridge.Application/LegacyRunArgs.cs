namespace K8sBridge.Application;

public record LegacyRunArgs(
    string Namespace,
    string Service,
    Map<string, int> PortMap);

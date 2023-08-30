namespace K8sBridge.Application;

public record RunArgs(
    string Namespace,
    string Service,
    Map<string, int> PortMap);

namespace K8sBridge.Application;

public record KubernetesService(
    string Namespace,
    string Name,
    Map<string, int> TargetPorts);

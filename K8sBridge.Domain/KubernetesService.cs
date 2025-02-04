using K8sBridge.Domain;

namespace K8sBridge.Application;

public record KubernetesService(
    string Namespace,
    string Name,
    Map<string, TargetPort> TargetPorts,
    Map<string, string> Selector);
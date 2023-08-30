namespace K8sBridge.Application;

public record KubernetesBridgePod(
    string Namespace,
    string Name,
    Map<string, int> Ports,
    int BridgePort);

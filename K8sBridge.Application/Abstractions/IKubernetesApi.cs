namespace K8sBridge.Application.Abstractions;

public interface IKubernetesApi
{
    ValueTask CreateBridgePod(KubernetesBridgePod pod, CancellationToken cancellationToken = default);

    ValueTask DeleteBridgePod(KubernetesBridgePod pod, CancellationToken cancellationToken = default);

    ValueTask<Option<KubernetesService>> FindServiceAsync(string @namespace, string name,
        CancellationToken cancellationToken = default);

    ValueTask PortforwardAsync(string @namespace, string name, int port, int localPort,
        CancellationToken cancellationToken = default);

    ValueTask UpdateServiceSelector(string @namespace, string name, Map<string, string> selector,
        CancellationToken cancellationToken = default);
}
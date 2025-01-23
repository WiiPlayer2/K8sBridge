using CliWrap;
using k8s;
using k8s.Models;
using K8sBridge.Application;
using K8sBridge.Application.Abstractions;

namespace K8sBridge.Implementations;

internal class KubernetesApi : IKubernetesApi
{
    private readonly IKubernetes k8s;

    public KubernetesApi()
    {
        var k8sConfig = KubernetesClientConfiguration.BuildDefaultConfig();
        k8s = new Kubernetes(k8sConfig);
    }

    public async ValueTask CreateBridgePod(KubernetesBridgePod pod, CancellationToken cancellationToken = default)
    {
        var createdPod = await k8s.CoreV1.CreateNamespacedPodAsync(
            new V1Pod(
                metadata: new V1ObjectMeta(
                    name: pod.Name,
                    labels: pod.Labels.ToDictionary(x => x.Key, x => x.Value)),
                spec: new V1PodSpec(new List<V1Container>
                {
                    new(
                        "frps",
                        image: "snowdreamtech/frps:0.51.3",
                        ports: pod.Ports
                            .Add("frps", pod.BridgePort)
                            .Pairs
                            .Select(x => new V1ContainerPort(x.Value, name: x.Key))
                            .ToList()),
                })),
            pod.Namespace,
            cancellationToken: cancellationToken);
        await k8s.CoreV1.ListNamespacedPodWithHttpMessagesAsync(
                createdPod.Namespace(),
                watch: true,
                cancellationToken: cancellationToken)
            .WatchAsync<V1Pod, V1PodList>(cancellationToken: cancellationToken)
            .Where(x => x.Item2.Metadata.Name == createdPod.Name())
            .FirstAsync(x => (x.Item2.Status.Conditions
                              ?? Enumerable.Empty<V1PodCondition>())
                    .Any(x => x is { Type: "Ready", Status: "True" }),
                cancellationToken);
    }

    public async ValueTask DeleteBridgePod(KubernetesBridgePod pod, CancellationToken cancellationToken = default)
    {
        await k8s.CoreV1.DeleteNamespacedPodAsync(pod.Name, pod.Namespace, cancellationToken: cancellationToken);
    }

    public async ValueTask<Option<KubernetesService>> FindServiceAsync(string @namespace, string name,
        CancellationToken cancellationToken = default)
    {
        var k8sService =
            await k8s.CoreV1.ReadNamespacedServiceAsync(name, @namespace, cancellationToken: cancellationToken);
        if (k8sService is null)
        {
            return None;
        }

        return new KubernetesService(
            k8sService.Namespace(),
            k8sService.Name(),
            k8sService.Spec.Ports
                .Select(x => (x.Name, int.Parse(x.TargetPort)))
                .ToMap(),
            k8sService.Spec.Selector.ToMap());
    }

    public async ValueTask PortforwardAsync(string @namespace, string name, int port, int localPort,
        CancellationToken cancellationToken = default)
    {
        await Cli.Wrap("kubectl")
            .WithArguments(b => b
                .Add("port-forward")
                .Add($"pods/{name}")
                .Add("-n")
                .Add(@namespace)
                .Add($"{localPort}:{port}"))
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteAsync(cancellationToken);
    }

    public async ValueTask UpdateServiceSelector(string @namespace, string name, Map<string, string> selector,
        CancellationToken cancellationToken = default)
    {
        var k8sService =
            await k8s.CoreV1.ReadNamespacedServiceAsync(name, @namespace, cancellationToken: cancellationToken);
        if (k8sService is null) throw new InvalidOperationException($"Service \"{@namespace}/{name}\" not found");

        k8sService.Spec.Selector = selector.AsEnumerable().ToDictionary();
        await k8s.CoreV1.ReplaceNamespacedServiceAsync(k8sService, name, @namespace,
            cancellationToken: cancellationToken);
    }
}
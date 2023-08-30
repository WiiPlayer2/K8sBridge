using K8sBridge.Application.Traits;

namespace K8sBridge.Application;

public class Entrypoint<RT>
    where RT : struct, HasCancel<RT>, HasApp<RT>
{
    public static Aff<RT, Unit> Run(RunArgs args) =>
        from _00 in unitEff
        from rt in runtime<RT>()
        from k8sApi in rt.KubernetesApiEff
        from k8sService in Aff((RT rt) => k8sApi.FindServiceAsync(args.Namespace, args.Service, rt.CancellationToken))
            .Bind(x => x.ToEff())
        let bridgePort = 7000
        let bridgePod = new KubernetesBridgePod(
            args.Namespace,
            $"tunneling-{args.Service}",
            k8sService.TargetPorts,
            bridgePort)
        let tunnelingPort = random(1000) + 7000
        from _10 in Aff((RT rt) => k8sApi.CreateBridgePod(bridgePod, rt.CancellationToken).ToUnit())
        from cancelTunneling in Aff((RT rt) => k8sApi.PortforwardAsync(bridgePod.Namespace, bridgePod.Name, bridgePort, tunnelingPort, rt.CancellationToken).ToUnit())
            .Fork()
        from tunnelingApi in rt.TunnelingApiEff
        from cancelPorts in bridgePod.Ports
            .Select(x => Aff((RT rt) => tunnelingApi.CreateTunnelAsync(args.PortMap[x.Key], x.Value).ToUnit()))
            .TraverseParallel(identity)
            .Fork()
        from _20 in Aff((RT rt) => Task.Delay(Timeout.Infinite).ToUnit().ToValue())
        // - Remove pod
        select unit;
}

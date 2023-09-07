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
            bridgePort,
            k8sService.Selector)
        let tunnelingPort = random(1000) + 7000
        from _10 in Aff((RT rt) => k8sApi.CreateBridgePod(bridgePod, rt.CancellationToken).ToUnit())
        from cancelTunneling in Aff((RT rt) => k8sApi.PortforwardAsync(bridgePod.Namespace, bridgePod.Name, bridgePort, tunnelingPort, rt.CancellationToken).ToUnit())
            .Fork()
        from tunnelingApi in rt.TunnelingApiEff
        from cancelPorts in bridgePod.Ports
            .Select(x => Aff((RT rt) => tunnelingApi.CreateTunnelAsync(tunnelingPort, x.Key, args.PortMap[x.Key], x.Value, rt.CancellationToken).ToUnit()))
            .TraverseParallel(identity)
            .Fork()
        from _20 in WaitForTermination()
            .Catch(Aff(() => k8sApi.DeleteBridgePod(bridgePod).ToUnit()))
        select unit;

    private static Aff<RT, Unit> WaitForTermination() =>
        Aff((RT rt) => Task.Delay(Timeout.Infinite, rt.CancellationToken).ToUnit().ToValue());
}

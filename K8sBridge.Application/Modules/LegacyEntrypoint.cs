using K8sBridge.Application.Traits;
using K8sBridge.Domain;
using LanguageExt.UnitsOfMeasure;

namespace K8sBridge.Application.Modules;

public class LegacyEntrypoint<RT>
    where RT : struct, HasCancel<RT>, HasApp<RT>
{
    public static Aff<RT, Unit> Run(LegacyRunArgs args) =>
        from _00 in unitEff
        from rt in runtime<RT>()
        from k8sApi in rt.KubernetesApiEff
        from k8sService in Aff((RT rt) => k8sApi.FindServiceAsync(args.Namespace, args.Service, rt.CancellationToken))
            .Bind(x => x.ToEff())
        let bridgeSelector = Map<string, string>(("k8s-bridge", args.Service))
        from _05 in Aff((RT rt) =>
            k8sApi.UpdateServiceSelector(args.Namespace, args.Service, bridgeSelector, rt.CancellationToken).ToUnit())
        let bridgePort = 7000
        let podPorts = DeterminePodPorts(k8sService.TargetPorts)
        let bridgePod = new KubernetesBridgePod(
            args.Namespace,
            $"tunneling-{args.Service}",
            podPorts,
            bridgePort,
            bridgeSelector)
        let tunnelingPort = random(1000) + 7000
        from _10 in Aff((RT rt) => k8sApi.CreateBridgePod(bridgePod, rt.CancellationToken).ToUnit())
        from cancelTunneling in Aff((RT rt) =>
                k8sApi.PortforwardAsync(bridgePod.Namespace, bridgePod.Name, bridgePort, tunnelingPort,
                    rt.CancellationToken).ToUnit())
            .Fork()
        from _15 in Aff((RT rt) => Task.Delay(5.Seconds(), rt.CancellationToken).ToUnit().ToValue())
        from tunnelingApi in rt.TunnelingApiEff
        from cancelPorts in bridgePod.Ports
            .Select(x => Aff((RT rt) =>
                tunnelingApi.CreateTunnelAsync(tunnelingPort, x.Key, args.PortMap[x.Key], x.Value, rt.CancellationToken)
                    .ToUnit()))
            .TraverseParallel(identity)
            .Fork()
        from _20 in WaitForTermination()
            .Catch(Aff(() => k8sApi.DeleteBridgePod(bridgePod).ToUnit()))
        from _30 in Aff(() =>
            k8sApi.UpdateServiceSelector(args.Namespace, args.Service, k8sService.Selector).ToUnit())
        select unit;

    private static Aff<RT, Unit> WaitForTermination() =>
        Aff((RT rt) => Task.Delay(Timeout.Infinite, rt.CancellationToken).ToUnit().ToValue());

    private static Map<string, int> DeterminePodPorts(Map<string, TargetPort> targetPorts)
    {
        var predeterminedPorts = targetPorts.Tuples
            .Select(x => x.Item2.Match(
                name => -1,
                number => number.Value))
            .Where(x => x != -1)
            .ToHashSet();
        var nextPort = 2000;
        var podPorts = targetPorts
            .Tuples
            .Map(port => port.Item2.Match(
                name =>
                {
                    var portNumber = nextPort;
                    while (predeterminedPorts.Contains(portNumber))
                        portNumber++;
                    nextPort = portNumber + 1;
                    return
                        (name.Value,
                            portNumber); // this assumes that the target port has the same name as the port name of the service
                },
                number => (port.Item1, number.Value)))
            .ToMap();
        return podPorts;
    }
}
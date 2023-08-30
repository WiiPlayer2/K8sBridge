using K8sBridge.Application.Abstractions;

namespace K8sBridge.Application.Traits;

public interface HasApp<RT>
    where RT : struct, HasApp<RT>
{
    Eff<RT, IKubernetesApi> KubernetesApiEff { get; }

    Eff<RT, ITunnelingApi> TunnelingApiEff { get; }
}

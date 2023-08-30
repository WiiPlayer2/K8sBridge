using K8sBridge.Application.Abstractions;
using K8sBridge.Application.Traits;

internal readonly struct Runtime :
    HasCancel<Runtime>,
    HasApp<Runtime>
{
    private readonly Env_? env;

    private Runtime(Env_ env)
    {
        this.env = env;
    }

    public Env_ Env => env ?? throw new InvalidOperationException();

    public Eff<Runtime, IKubernetesApi> KubernetesApiEff => Env.KubernetesApiEff;

    public Eff<Runtime, ITunnelingApi> TunnelingApiEff => Env.TunnelingApiEff;

    public CancellationToken CancellationToken => Env.CancellationToken;

    public CancellationTokenSource CancellationTokenSource => Env.CancellationTokenSource;

    public Runtime LocalCancel =>
        Env.Apply(env =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(env.CancellationToken);
            return New(env with
            {
                CancellationToken = cts.Token,
                CancellationTokenSource = cts,
            });
        });

    public static Runtime New(Env_ env) => new(env);

    public static Runtime New(
        CancellationToken cancellationToken,
        Eff<Runtime, IKubernetesApi> kubernetesApiEff,
        Eff<Runtime, ITunnelingApi> tunnelingApiEff)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        return New(new Env_(
            cts.Token,
            cts,
            kubernetesApiEff,
            tunnelingApiEff));
    }

    public record Env_(
        CancellationToken CancellationToken,
        CancellationTokenSource CancellationTokenSource,
        Eff<Runtime, IKubernetesApi> KubernetesApiEff,
        Eff<Runtime, ITunnelingApi> TunnelingApiEff);
}

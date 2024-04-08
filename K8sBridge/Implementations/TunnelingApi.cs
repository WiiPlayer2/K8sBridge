using CliWrap;
using K8sBridge.Application.Abstractions;

namespace K8sBridge.Implementations;

internal class TunnelingApi : ITunnelingApi
{
    public async ValueTask CreateTunnelAsync(int tunnelingPort, string tunnelingName, int localPort, int remotePort, CancellationToken cancellationToken = default)
    {
        await Cli.Wrap("frpc")
            .WithArguments(b => b
                .Add("tcp")
                .Add("--proxy-name")
                .Add(tunnelingName)
                .Add("--local-port")
                .Add(localPort)
                .Add("--remote-port")
                .Add(remotePort)
                .Add("--server-addr")
                .Add("localhost")
                .Add("--server-port")
                .Add(tunnelingPort))
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteAsync(cancellationToken);
    }
}

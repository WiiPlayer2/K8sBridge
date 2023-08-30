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
                .Add("--proxy_name")
                .Add(tunnelingName)
                .Add("--local_port")
                .Add(localPort)
                .Add("--remote_port")
                .Add(remotePort)
                .Add("--server_addr")
                .Add($"127.0.0.1:{tunnelingPort}"))
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteAsync(cancellationToken);
    }
}

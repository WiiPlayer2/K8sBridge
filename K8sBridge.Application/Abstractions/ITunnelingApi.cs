namespace K8sBridge.Application.Abstractions;

public interface ITunnelingApi
{
    ValueTask CreateTunnelAsync(int localPort, int remotePort, CancellationToken cancellationToken = default);
}

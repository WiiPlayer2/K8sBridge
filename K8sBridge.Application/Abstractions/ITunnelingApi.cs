namespace K8sBridge.Application.Abstractions;

public interface ITunnelingApi
{
    ValueTask CreateTunnelAsync(int tunnelingPort, string tunnelingName, int localPort, int remotePort, CancellationToken cancellationToken = default);
}

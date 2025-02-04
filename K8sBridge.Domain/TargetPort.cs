using FunicularSwitch.Generators;

namespace K8sBridge.Domain;

[UnionType]
public abstract partial record TargetPort
{
    public record Name_(string Value) : TargetPort;

    public record Number_(int Value) : TargetPort;
}
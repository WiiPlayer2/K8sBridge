using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using K8sBridge.Application;
using K8sBridge.Application.Abstractions;
using K8sBridge.Application.Modules;
using K8sBridge.Implementations;

var legacyCommand = new Command("legacy")
{
    new System.CommandLine.Option<string>(new[] {"--namespace", "-n",}),
    new Argument<string>("service"),
    new System.CommandLine.Option<string[]>(new[] {"--port-name", "-o",}),
    new System.CommandLine.Option<int[]>(new[] {"--port", "-p",}),
};
legacyCommand.Handler = CommandHandler.Create(InvokeLegacy);

var rootCommand = new RootCommand
{
    legacyCommand,
};

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .Build();
return await parser.InvokeAsync(args);

async Task InvokeLegacy(
    string @namespace,
    string service,
    string[] portName,
    int[] port,
    CancellationToken cancellationToken) =>
    await LegacyEntrypoint<Runtime>
        .Run(new LegacyRunArgs(
            @namespace,
            service,
            portName.Zip(port).ToMap()))
        .RunUnit(BuildRuntime(cancellationToken));

Runtime BuildRuntime(CancellationToken cancellationToken) =>
    Runtime.New(
        cancellationToken,
        Eff<IKubernetesApi>(() => new KubernetesApi()),
        Eff<ITunnelingApi>(() => new TunnelingApi()));

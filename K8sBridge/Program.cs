using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using K8sBridge.Application;

var namespaceOption = new System.CommandLine.Option<string>(new[] {"--namespace", "-n"});
var serviceArgument = new Argument<string>("service");
var portNamesOption = new System.CommandLine.Option<string[]>(new[] {"--port-name", "-o"});
var portsOption = new System.CommandLine.Option<int[]>(new[] {"--port", "-p"});

var rootCommand = new RootCommand
{
    namespaceOption,
    serviceArgument,
    portNamesOption,
    portsOption,
};
rootCommand.SetHandler(ExecuteInRuntime);

var parser = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .Build();
return await parser.InvokeAsync(args);

async Task ExecuteInRuntime(InvocationContext ctx)
{
    var runtime = Runtime.New(
        ctx.GetCancellationToken(),
        default,
        default);
    var runArgs = new RunArgs(
        ctx.ParseResult.GetValueForOption(namespaceOption) ?? throw new ArgumentNullException("namespace"),
        ctx.ParseResult.GetValueForArgument(serviceArgument) ?? throw new ArgumentNullException("service"),
        (ctx.ParseResult.GetValueForOption(portNamesOption) ?? throw new ArgumentNullException("portNames"))
        .Zip(ctx.ParseResult.GetValueForOption(portsOption))
        .ToMap());
    await Entrypoint<Runtime>.Run(runArgs).RunUnit(runtime);
}

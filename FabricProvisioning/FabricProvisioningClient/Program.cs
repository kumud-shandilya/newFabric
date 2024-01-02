// See https://aka.ms/new-console-template for more information.
using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Logging;
using Microsoft.Fabric.Provisioning.Library.Models;
using ProvisioningLibrary = Microsoft.Fabric.Provisioning.Library;

// Service collection for configuring dependency injection.
var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);
var serviceProvider = serviceCollection.BuildServiceProvider();

// Get an instance of the operations class.
var operations = serviceProvider.GetService<ProvisioningLibrary.Operations>();
var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("Microsoft.Fabric.Provisioning.Client");

var tokenOption = new Option<string>(
            name: "--token",
            description: "The access token to authentice.")
            { IsRequired = true };

var payloadOption = new Option<ProvisioningLibrary.Models.WorkspaceRequest?>(
            name: "--payload",
            description: "The request payload or body.",
            parseArgument: result =>
                         {
                             if (result.Tokens.Count > 0)
                             {
                                 return JsonSerializer.Deserialize<WorkspaceRequest>(result.Tokens.FirstOrDefault().Value);
                             }
                             else
                             { 
                                 result.ErrorMessage = "--payload is empty.";
                                 return default;
                             }
                         })
            { IsRequired = true };

var correlationIdOption = new Option<string>(
            name: "--correlationId",
            description: "The correlation id associated with the operation.")
            { IsRequired = false };

var rootCommand = new RootCommand("Application for Microsoft Fabric Provisioning.");

var createCommand = new Command("create", "Creates workspace and associated workload.")
{
    tokenOption,
    payloadOption,
    correlationIdOption
};
rootCommand.AddCommand(createCommand);

createCommand.SetHandler((token, payload, correlationId) =>
{
    var response = payload != null ? operations?.Create(token, payload) : default;
    if (response != null)
    {
        logger?.LogInformation(JsonSerializer.Serialize<WorkspaceResponse>(response));
    }
    else
    {
        logger?.LogError(500, "No response found.");
    }
},
tokenOption, payloadOption, correlationIdOption);

return await rootCommand.InvokeAsync(args);

static void ConfigureServices(ServiceCollection services) =>
    services.AddLogging(config =>
    {
        config.AddDebug();
        config.AddConsole();
    })
    .Configure<LoggerFilterOptions>(options =>
    {
        options.AddFilter<DebugLoggerProvider>("FabricProvisioning", LogLevel.Information);
        options.AddFilter<ConsoleLoggerProvider>("FabricProvisioning", LogLevel.Warning);
    })
    .AddTransient<ProvisioningLibrary.Operations>();